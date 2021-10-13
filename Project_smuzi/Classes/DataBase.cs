using Kompas6Constants;
using KompasAPI7;
using Newtonsoft.Json;
using Project_smuzi.Models;
using Project_smuzi.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
#pragma warning disable CS0067
namespace Project_smuzi.Classes
{
    public class DataBase : INotifyPropertyChanged
    {
        [JsonIgnore]
        private ObservableCollection<Product> productes;
        [JsonIgnore]
        private ObservableCollection<Element> elementes;

        public DataBase()
        {
            Productes = new ObservableCollection<Product>();
            Elementes = new ObservableCollection<Element>();
            Element.Identificator = 1;
        }
        public DataBase(DataBase db)
        {
            Productes = db.Productes;
            Elementes = db.Elementes;
            Prefix = db.Prefix;
        }
        public DataBase Copy()
        {
            return new DataBase()
            {
                Productes = this.Productes,
                Elementes = this.Elementes,
                Prefix = this.Prefix
            };

        }
        public ObservableCollection<Product> Productes { get => productes; set { productes = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Productes")); } }
        public ObservableCollection<Element> Elementes { get => elementes; set { elementes = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Elementes")); } }
        [JsonIgnore]
        public ObservableCollection<int> DeepList => new ObservableCollection<int>(Productes.Select(t => t.DeepLevel).Distinct().OrderBy(t => t));

        public event PropertyChangedEventHandler PropertyChanged;

        public void Clear()
        {
            Productes = new ObservableCollection<Product>();
            Elementes = new ObservableCollection<Element>();
            Element.Identificator = 1;
        }


        public string Prefix { get; set; }


        #region DataBaseLoader
        public static BackgroundWorker worker = new BackgroundWorker() { WorkerReportsProgress = true };

        public static void TestSpwReader(string FolderPath, string Prefix)
        {
            DataBase db = SharedModel.DB;
            db.Clear();
            db.Prefix = Prefix;
            //ReadDataDone += DB_ReadDataDone;
            IApplication kmpsApp = null;
            Type t7 = Type.GetTypeFromProgID("KOMPAS.Application.7");
            try
            {
                kmpsApp = (IApplication)Activator.CreateInstance(t7);
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("ERROR open KOMPASS");
                SharedModel.InvokeReadDataDone(db);
            }
            if (kmpsApp == null)
            {
                System.Windows.Forms.MessageBox.Show("ERROR open KOMPASS");
            }
            //ЕСЛИ БУДЕТ ДОПОЛНЕНИЕ БАЗЫ, то удалить
            //db.Clear();


            kmpsApp.HideMessage = ksHideMessageEnum.ksHideMessageYes;


            var all_dir = Directory.GetFiles(FolderPath, "*sp.spw", SearchOption.AllDirectories).ToList();
            int progerss = 0;
            int iter = all_dir.Count / 100;

            foreach (var item in all_dir)
            {
                try
                {
                    IKompasDocument kmpsdoc = kmpsApp.Documents.Open(item);
                    var spec_descript = kmpsdoc.SpecificationDescriptions;
                    string name = "";
                    string ident = "";
                    if (kmpsdoc.LayoutSheets.Count > 0)
                    {
                        ident = kmpsdoc.LayoutSheets[0].Stamp.Text[0].Str.Trim();
                        name = kmpsdoc.LayoutSheets[0].Stamp.Text[1].Str.Trim();
                        name = name.Replace("\n", " ");
                        ident = ident.Replace("\n", " ");
                    }
                    //
                    if (!ident.Contains(db.Prefix))
                    {
                        var st = kmpsdoc.LayoutSheets[0].Stamp.Text[2].Str.Trim();
                        if (st.Contains(db.Prefix))
                            ident = st;
                        else
                            ident = Path.GetFileNameWithoutExtension(item);//ТСЮИ.466225.006sp
                    }

                    Product product = null;
                    var buf = db.Productes.Where(t => t.Identification == ident).FirstOrDefault();
                    if (buf == null) //если нет в спсике изделий
                    {
                        product = new Product(ident) { Name = name, Count = 1, Section_id = 15, DeepLevel = 0, PathTo = item };
                        db.Productes.Add(product); //добавляем изделие
                    }
                    else
                    {
                        product = buf;
                        if (string.IsNullOrEmpty(product.PathTo))
                        {
                            product.PathTo = item;
                        }
                    }

                    var newTEST = (IList)spec_descript.Active.Objects;

                    //var cnt = spec_descript.Active.BaseObjects.Count + spec_descript.Active.CommentObjects.Count;
                    string helpfullstr_naimen = string.Empty;
                    string helpfullstr_oboz = string.Empty;
                    for (int i = 0; i < newTEST.Count; i++)
                    {
                        try
                        {

                            ISpecificationObject ispecobj = (ISpecificationObject)newTEST[i];
                            ksSpecificationObjectTypeEnum type = ispecobj.ObjectType;

                            /*
 * проблемы
 * 1. узнаем раздел ли это криво добавленный
 *  - если след строка пустая и все поля пустые кроме наименования и объект вспомогательный (? и предыдущая пустая ?)
 *  то раздел
 * 2. если строка след. не пустая, то это наименование обобщение. его не добавляем, а просто вхуяриваем дальше всем тем кто снизу 
 * то что в ней
 * до каких то пор
 * наверное до тех, пока не будет такая же отбитая строка, или пустая
 * 
 */
                            bool isadd = false;
                            switch (type)
                            {
                                case ksSpecificationObjectTypeEnum.ksSpecificationBaseObject:
                                    isadd = false;
                                    break;
                                case ksSpecificationObjectTypeEnum.ksSpecificationComment:
                                    isadd = true;
                                    break;
                                case ksSpecificationObjectTypeEnum.ksSpecificationUnknownObject:
                                case ksSpecificationObjectTypeEnum.ksSpecificationSectionName:
                                case ksSpecificationObjectTypeEnum.ksSpecificationBlock:
                                case ksSpecificationObjectTypeEnum.ksSpecificationReserveString:
                                case ksSpecificationObjectTypeEnum.ksSpecificationEmptyString:
                                    continue;
                            }
                            string format, zone, poz, naimenovanie, oboznachenie, comment;

                            SpecificationColumn b = ispecobj.Columns.Column[ksSpecificationColumnTypeEnum.ksSColumnFormat, 1, 0];
                            if (b != null)
                                format = b.Text.Str.Trim();//   # формат
                            else
                                format = string.Empty;

                            b = ispecobj.Columns.Column[ksSpecificationColumnTypeEnum.ksSColumnZone, 1, 0];
                            if (b != null)
                                zone = b.Text.Str.Trim();//   # зона
                            else
                                zone = string.Empty;
                            b = ispecobj.Columns.Column[ksSpecificationColumnTypeEnum.ksSColumnPosition, 1, 0];
                            if (b != null)
                                poz = b.Text.Str.Trim();//   # позиция
                            else
                                poz = string.Empty;
                            b = ispecobj.Columns.Column[ksSpecificationColumnTypeEnum.ksSColumnName, 1, 0];
                            if (b != null)
                                naimenovanie = b.Text.Str.Trim();//   # Наименование
                            else
                                naimenovanie = string.Empty;
                            b = ispecobj.Columns.Column[ksSpecificationColumnTypeEnum.ksSColumnMark, 1, 0];
                            if (b != null)
                                oboznachenie = b.Text.Str.Trim();//   # Обозначение
                            else
                                oboznachenie = string.Empty;
                            b = ispecobj.Columns.Column[ksSpecificationColumnTypeEnum.ksSColumnNote, 1, 0];
                            if (b != null)
                                comment = b.Text.Str.Trim();//   # примечание
                            else
                                comment = string.Empty;

                            double.TryParse(ispecobj.Columns.Column[ksSpecificationColumnTypeEnum.ksSColumnCount, 1, 0].Text.Str, out double kolichestvo);//   # кол-во   


                            naimenovanie = naimenovanie.Replace("\t", " ");
                            oboznachenie = oboznachenie.Replace("\t", " ");
                            naimenovanie = naimenovanie.Replace("\n", " ");
                            oboznachenie = oboznachenie.Replace("\n", " ");

                            naimenovanie = naimenovanie.Replace(Environment.NewLine, " ");
                            oboznachenie = oboznachenie.Replace(Environment.NewLine, " ");





                            if (string.IsNullOrWhiteSpace(format + zone + poz + oboznachenie + kolichestvo + comment) && isadd) //признак всяких трубок криворуких
                            {
                                if ((i + 1) < newTEST.Count) //если след строка существует
                                {
                                    ISpecificationObject ispecobj_dop = (ISpecificationObject)newTEST[i];
                                    if (ispecobj.ObjectType == ksSpecificationObjectTypeEnum.ksSpecificationBaseObject ||
                                        ispecobj.ObjectType == ksSpecificationObjectTypeEnum.ksSpecificationComment)
                                    {
                                        helpfullstr_naimen = naimenovanie;
                                    }
                                    else
                                    {
                                        if (System.Windows.Forms.MessageBox.Show($"ВОЗМОЖНАЯ категория новая {naimenovanie}", "", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                                        {
                                            int vb = 0;
                                        }
                                    }

                                }
                                continue;
                            }

                            if (!Regex.Match(naimenovanie, @"^\D{3,}.+$").Success) //провека на вменяемость названия
                            {
                                naimenovanie = helpfullstr_naimen + " " + naimenovanie;
                            }

                            if (Regex.Match(oboznachenie, @"^-\d{1,}$").Success)
                                //определяем что строка = -01 и тд
                                oboznachenie = helpfullstr_oboz + oboznachenie;
                            else
                                helpfullstr_oboz = oboznachenie;


                            //if (string.IsNullOrEmpty(naimenovanie) & string.IsNullOrEmpty(oboznachenie))
                            //   continue;

                            if (string.IsNullOrEmpty(poz))
                            {
                                //заглушка дабы не залетало то чего нет в позициях (документы и прочее барахло)
                                continue;
                            }





                            var sect_num = ispecobj.Section;//aw.Section; // id секции в которой находится
                            if (sect_num == 0)
                                sect_num = ispecobj.AdditionalSection;
                            Placer(product, sect_num, naimenovanie, oboznachenie, kolichestvo, isadd, db);

                        }
                        catch (Exception)
                        {

                            throw;
                        }
                    }

                    //var iSpecificationBaseObjects = spec_descript.Active.BaseObjects;
                    //for (int i = 0; i < iSpecificationBaseObjects.Count; i++)
                    //{
                    //    try
                    //    {
                    //        InnerVorker_Base(iSpecificationBaseObjects[i], product, db);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        if (ex.HResult == 101)
                    //            SharedModel.InvokeLogSend($"{ex.Message}\n{product.Identification}\nБазовый объект спецификации. Строка № {i}");
                    //        else
                    //            throw ex;
                    //    }
                    //}
                    //var iSpecificationCommentObjects = spec_descript.Active.CommentObjects;
                    //for (int i = 0; i < iSpecificationCommentObjects.Count; i++)
                    //{
                    //    InnerVorker_Additioanl(iSpecificationCommentObjects[i], product, db);
                    //}

                    kmpsdoc.Close(DocumentCloseOptions.kdDoNotSaveChanges);
                    db.PropertyChanged?.Invoke(db, new PropertyChangedEventArgs("DB"));
                    //Selector = HeavyProducts;
                    SharedModel.InvokeLogSend($"doc {ident} is {name} work done");
                    //await Task.Run(() => DataBase.WorkProcStep?.//Invoke(db)); 
                    //WorkProcStep?.Invoke(db);
                    //SharedModel.DB = db.Copy();
                    progerss += iter;
                    worker.ReportProgress(progerss, db);

                }
                catch (Exception ex)
                {
                    SharedModel.InvokeLogSend($"{ex.Message}\ndoc {item} open error");
                }

            }
            Settings.Default.DB_json = JsonConvert.SerializeObject(db, Formatting.Indented);
            Settings.Default.Save();
            Settings.Default.Reload();
            SharedModel.InvokeLogSend($"Save and done {all_dir.Count} documents");
            GC.Collect();
            kmpsApp.Quit();
            SharedModel.InvokeReadDataDone(db);
        }

        private static void InnerVorker_Base(ISpecificationBaseObject iSepcObj, Product base_product, DataBase db)
        {
            //если соответствует префиксу = изделие.
            //Ищем в списке изделий.
            //Если есть то берем то что нашли и пихаем в текущее изделие в его изделия.
            //Если нет, создаем изделие, пихаем в спсиок издейли, и в текущее в его изделия.
            var sect_num = iSepcObj.Section;//aw.Section; // id секции в которой находится
            if (sect_num == 0)
                sect_num = iSepcObj.AdditionalSection;
            var naimenovanie = iSepcObj.Columns.Column[ksSpecificationColumnTypeEnum.ksSColumnName, 1, 0].Text.Str.Trim();//   # 5 - колонка "Наименование"
            var oboznachenie = iSepcObj.Columns.Column[ksSpecificationColumnTypeEnum.ksSColumnMark, 1, 0].Text.Str.Trim();//   # Обозначение
            naimenovanie = naimenovanie.Replace("\n", " ");
            oboznachenie = oboznachenie.Replace("\n", " ");
            if (string.IsNullOrEmpty(naimenovanie) & string.IsNullOrEmpty(oboznachenie))
                return;
            double.TryParse(iSepcObj.Columns.Column[ksSpecificationColumnTypeEnum.ksSColumnCount, 1, 0].Text.Str, out double kolichestvo);//   # кол-во   
            Placer(base_product, sect_num, naimenovanie, oboznachenie, kolichestvo, false, db);
        }
        private static void InnerVorker_Additioanl(ISpecificationCommentObject iSepcObj, Product base_product, DataBase db)
        {
            var sect_num = iSepcObj.Section;//aw.Section; // id секции в которой находится
            if (sect_num == 0)
                sect_num = iSepcObj.AdditionalSection;
            var naimenovanie = iSepcObj.Columns.Column[ksSpecificationColumnTypeEnum.ksSColumnName, 1, 0].Text.Str.Trim();//        # Наименование
            var oboznachenie = iSepcObj.Columns.Column[ksSpecificationColumnTypeEnum.ksSColumnMark, 1, 0].Text.Str.Trim();//        # Обозначение
            naimenovanie = naimenovanie.Replace("\n", " ");
            oboznachenie = oboznachenie.Replace("\n", " ");
            if (string.IsNullOrEmpty(naimenovanie) & string.IsNullOrEmpty(oboznachenie))
                return;
            double.TryParse(iSepcObj.Columns.Column[ksSpecificationColumnTypeEnum.ksSColumnCount, 1, 0].Text.Str, out double kolichestvo);//  # кол-во   
            Placer(base_product, sect_num, naimenovanie, oboznachenie, kolichestvo, true, db);
        }
        private static void Placer(Product base_product, int sect_num, string naimenovanie, string oboznachenie, double kolichestvo, bool isadditional, DataBase db)
        {
            Debug.WriteLine(kolichestvo);
            if (oboznachenie.Contains(db.Prefix))//item.Contains(Prefix))
            {
                if ("Документация" != SharedModel.GetInterpritation(sect_num))
                    AddProduct(oboznachenie, naimenovanie, sect_num, kolichestvo, base_product, isadditional, db);
                else
                    AddElement(oboznachenie, naimenovanie, sect_num, kolichestvo, base_product, isadditional, db);
            }
            else
                AddElement(oboznachenie, naimenovanie, sect_num, kolichestvo, base_product, isadditional, db);
        }
        private static void AddElement(string oboznachenie, string naimenovanie, int section_num, double kolichestvo, Product base_product, bool isAdd, DataBase db)
        {
            //ЕЛЕМЕНТ
            Element element_in = null;
            var buf_in = db.Elementes.Where(t => t.Name == naimenovanie).FirstOrDefault();
            //если нет в спсике елементов
            if (buf_in != null)
            {
                element_in = buf_in;
                element_in.Count += kolichestvo;
            }
            else
            {
                element_in = new Element(oboznachenie) { Name = naimenovanie, Count = kolichestvo, Section_id = section_num, IsAdditional = isAdd };
                db.Elementes.Add(element_in); //добавляем елемент
            }
            if (!element_in.Contaiments_in.Contains(base_product.BaseId))
                element_in.Contaiments_in.Add(base_product.BaseId);
            //Если есть такой элемент внутри издеия, то увеличиваем кол-во, если нет добавляем
            //var in_in = base_product.Elements.Where(t => t.Name == naimenovanie).FirstOrDefault();
            if (base_product.Contaiment.ContainsKey(element_in.BaseId))
            {
                base_product.Contaiment[element_in.BaseId] += kolichestvo;
            }
            else
            {
                base_product.Contaiment.Add(element_in.BaseId, kolichestvo);
            }
        }
        private static void AddProduct(string oboznachenie, string naimenovanie, int section_num, double kolichestvo, Product base_product, bool isAdd, DataBase db)
        {
            Product product_in = null;
            var buf_in = db.Productes.Where(t => t.Identification == oboznachenie).FirstOrDefault();
            if (buf_in != null)
            {
                product_in = buf_in;
                if (product_in.BaseId == base_product.BaseId)
                    throw new Exception($"Ошибка чтения строки спецификации\n" +
                        $"Обозначение {oboznachenie} | Наименование {naimenovanie} | Кол. {kolichestvo} | Раздел {product_in.Section}")
                    { HResult = 101 };
            }
            else
            {
                product_in = new Product(oboznachenie) { Name = naimenovanie, Count = kolichestvo, Section_id = section_num, IsAdditional = isAdd };
                db.Productes.Add(product_in); //добавляем изделие
            }

            var a = product_in.DeepLevel < (base_product.DeepLevel + 1) ? (base_product.DeepLevel + 1) : product_in.DeepLevel;
            if (a != product_in.DeepLevel)
            {
                product_in.GoingDeeper();
                product_in.DeepLevel = a;
            }


            product_in.Contaiments_in.Add(base_product.BaseId);

            if (base_product.Contaiment.ContainsKey(product_in.BaseId))
            {
                base_product.Contaiment[product_in.BaseId] += kolichestvo;
            }
            else
            {
                base_product.Contaiment.Add(product_in.BaseId, kolichestvo);
            }
            //Если есть такое изделие внутри издеия, то увеличиваем кол-во, если нет добавляем
            //var in_in = base_product.Products.Where(t => t.Identification == oboznachenie).FirstOrDefault();
            //if (in_in == null)
            //{
            //    var buf_prd = product_in.Copy();
            //    buf_prd.Count = kolichestvo;
            //    //base_product.Products.Add(buf_prd);
            //    base_product.Contaiment.Add(buf_prd.BaseId, buf_prd.Count);
            //}
            //else //потом посчитать кол-во общее вдруг где то используется много какой нибудь херни
            //    in_in.Count += kolichestvo;
        }
        #endregion

    }
}
