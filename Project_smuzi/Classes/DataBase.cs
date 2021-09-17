using Kompas6Constants;
using KompasAPI7;
using Newtonsoft.Json;
using Project_smuzi.Models;
using Project_smuzi.Properties;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        public ObservableCollection<Product> Productes { get => productes; set { productes = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Productes")); } }
        public ObservableCollection<Element> Elementes { get => elementes; set { elementes = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Elementes")); } }

        [JsonIgnore]
        public ObservableCollection<Product> HeavyProducts => new ObservableCollection<Product>(Productes.Where(t => t.Elements.Count > 0 && t.Products.Count > 0));
        [JsonIgnore]
        public ObservableCollection<int> DeepList => new ObservableCollection<int>(Productes.Select(t => t.DeepLevel).Distinct().OrderBy(t => t));
        [JsonIgnore]
        public ObservableCollection<Product> Selector { get => selector; set { selector = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Selector")); } }
        [JsonIgnore]
        private ObservableCollection<Product> selector;

        public event PropertyChangedEventHandler PropertyChanged;

        public void LoadFromContaiment()
        {
            foreach (var prd in Productes)
            {
                foreach (var item in prd.Contaiment)
                {
                    var q = Productes.Where(t => t.BaseId == item.Key).FirstOrDefault();
                    if (q == null)
                    {
                        var z = Elementes.Where(t => t.BaseId == item.Key).FirstOrDefault();
                        z = z.Copy();
                        z.Count = item.Value;
                        prd.Elements.Add(z);
                    }
                    else
                    {
                        q = q.Copy();
                        q.Count = item.Value;
                        prd.Products.Add(q);
                    }
                }
            }
        }
        public void Clear()
        {
            Productes = new ObservableCollection<Product>();
            Elementes = new ObservableCollection<Element>();
            Selector = new ObservableCollection<Product>();
            Element.Identificator = 1;
        }


        public string Prefix { get; set; }

        #region DataBaseLoader
        public void TestSpwReader(string FolderPath)
        {
            this.Clear();
            //ReadDataDone += DB_ReadDataDone;
            IApplication kmpsApp = null;
            Type t7 = Type.GetTypeFromProgID("KOMPAS.Application.7");
            kmpsApp = (IApplication)Activator.CreateInstance(t7);
            if (kmpsApp == null)
            {
                System.Windows.Forms.MessageBox.Show("ERROR open KOMPASS");
                return;
            }
            kmpsApp.HideMessage = ksHideMessageEnum.ksHideMessageYes;

            var all_dir = Directory.GetFiles(FolderPath, "*sp.spw*", SearchOption.AllDirectories).ToList();
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
                    }
                    //
                    if (!ident.Contains(Prefix))
                    {
                        var st = kmpsdoc.LayoutSheets[0].Stamp.Text[2].Str.Trim();
                        if (st.Contains(Prefix))
                            ident = st;
                        else
                            ident = Path.GetFileNameWithoutExtension(item);//ТСЮИ.466225.006sp
                    }

                    Product product = null;
                    var buf = Productes.Where(t => t.Identification == ident).FirstOrDefault();
                    if (buf == null) //если нет в спсике изделий
                    {
                        product = new Product(ident) { Name = name, Count = 1, Section_id = 15, DeepLevel = 0, PathTo = item };
                        Productes.Add(product); //добавляем изделие
                    }
                    else
                    {
                        product = buf;
                        if (string.IsNullOrEmpty(product.PathTo))
                        {
                            product.PathTo = item;
                            var l = product.Contaiments_in.ToList();
                            foreach (var prd in l)
                            {
                                Productes.Where(t => t.BaseId == prd).FirstOrDefault().Products.Where(t => t.BaseId == product.BaseId).FirstOrDefault().PathTo = item;
                            }
                        }
                    }

                    var iSpecificationBaseObjects = spec_descript.Active.BaseObjects;
                    for (int i = 0; i < iSpecificationBaseObjects.Count; i++)
                    {
                        try
                        {
                            InnerVorker_Base(iSpecificationBaseObjects[i], product);
                        }
                        catch (Exception ex)
                        {
                            if (ex.HResult == 101)
                                Debug.WriteLine($"{ex.Message}\n{product.Identification}\nБазовый объект спецификации. Строка № {i}");
                            else
                                throw ex;
                        }
                    }
                    var iSpecificationCommentObjects = spec_descript.Active.CommentObjects;
                    for (int i = 0; i < iSpecificationCommentObjects.Count; i++)
                    {
                        InnerVorker_Additioanl(iSpecificationCommentObjects[i], product);
                    }
                    kmpsdoc.Close(DocumentCloseOptions.kdDoNotSaveChanges);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DB"));
                    Selector = HeavyProducts;
                    Debug.WriteLine($"doc {ident} is {name} work done");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{ex.Message}\ndoc {item} open error");
                }
            }
            Settings.Default.DB_json = JsonConvert.SerializeObject(this, Formatting.Indented);
            Settings.Default.Save();
            Settings.Default.Reload();
            Debug.WriteLine($"Save and done {all_dir.Count} documents");
            SharedModel.InvokeReadDataDone();
            GC.Collect();
            kmpsApp.Quit();
        }
        private void InnerVorker_Base(ISpecificationBaseObject iSepcObj, Product base_product)
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
            if (string.IsNullOrEmpty(naimenovanie) & string.IsNullOrEmpty(oboznachenie))
                return;
            double kolichestvo = 0;
            double.TryParse(iSepcObj.Columns.Column[ksSpecificationColumnTypeEnum.ksSColumnCount, 1, 0].Text.Str, out kolichestvo);//   # кол-во   
            Placer(base_product, sect_num, naimenovanie, oboznachenie, kolichestvo);
        }
        private void InnerVorker_Additioanl(ISpecificationCommentObject iSepcObj, Product base_product)
        {
            var sect_num = iSepcObj.Section;//aw.Section; // id секции в которой находится
            if (sect_num == 0)
                sect_num = iSepcObj.AdditionalSection;
            var naimenovanie = iSepcObj.Columns.Column[ksSpecificationColumnTypeEnum.ksSColumnName, 1, 0].Text.Str.Trim();//        # Наименование
            var oboznachenie = iSepcObj.Columns.Column[ksSpecificationColumnTypeEnum.ksSColumnMark, 1, 0].Text.Str.Trim();//        # Обозначение
            if (string.IsNullOrEmpty(naimenovanie) & string.IsNullOrEmpty(oboznachenie))
                return;
            double kolichestvo = 0;
            double.TryParse(iSepcObj.Columns.Column[ksSpecificationColumnTypeEnum.ksSColumnCount, 1, 0].Text.Str, out kolichestvo);//  # кол-во   
            Placer(base_product, sect_num, naimenovanie, oboznachenie, kolichestvo);
        }
        private void Placer(Product base_product, int sect_num, string naimenovanie, string oboznachenie, double kolichestvo)
        {
            if (oboznachenie.Contains(Prefix))//item.Contains(Prefix))
            {
                if ("Документация" != Element.GetInterpritation(sect_num))
                    AddProduct(oboznachenie, naimenovanie, sect_num, kolichestvo, base_product);
                else
                    AddElement(oboznachenie, naimenovanie, sect_num, kolichestvo, base_product);
            }
            else
                AddElement(oboznachenie, naimenovanie, sect_num, kolichestvo, base_product);
        }
        private void AddElement(string oboznachenie, string naimenovanie, int section_num, double kolichestvo, Product base_product)
        {
            //ЕЛЕМЕНТ
            Element element_in = null;
            var buf_in = Elementes.Where(t => t.Name == naimenovanie).FirstOrDefault();
            if (buf_in == null) //если нет в спсике елементов
            {
                element_in = new Element(oboznachenie) { Name = naimenovanie, Count = kolichestvo, Section_id = section_num };
                Elementes.Add(element_in); //добавляем елемент
            }
            else
            {
                element_in = buf_in;
                element_in.Count += kolichestvo;
            }
            element_in.Contaiments_in.Add(base_product.BaseId);
            //Если есть такой элемент внутри издеия, то увеличиваем кол-во, если нет добавляем
            var in_in = base_product.Elements.Where(t => t.Name == naimenovanie).FirstOrDefault();
            if (in_in == null)
            {
                var buf_el = element_in.Copy();
                buf_el.Count = kolichestvo;
                base_product.Elements.Add(buf_el);
                base_product.Contaiment.Add(buf_el.BaseId, buf_el.Count);
            }
            else
                in_in.Count += kolichestvo;
        }
        private void AddProduct(string oboznachenie, string naimenovanie, int section_num, double kolichestvo, Product base_product)
        {
            Product product_in = null;
            var buf_in = Productes.Where(t => t.Identification == oboznachenie).FirstOrDefault();
            if (buf_in == null) //если нет в спсике изделий
            {
                product_in = new Product(oboznachenie) { Name = naimenovanie, Count = kolichestvo, Section_id = section_num };
                Productes.Add(product_in); //добавляем изделие
            }
            else
            {
                product_in = buf_in;
                if (product_in.BaseId == base_product.BaseId)
                    throw new Exception("Ошибка чтения строки спецификации") { HResult = 101 };
            }
            product_in.DeepLevel = product_in.DeepLevel < (base_product.DeepLevel + 1) ? base_product.DeepLevel + 1 : product_in.DeepLevel;

            product_in.Contaiments_in.Add(base_product.BaseId);
            //Если есть такое изделие внутри издеия, то увеличиваем кол-во, если нет добавляем
            var in_in = base_product.Products.Where(t => t.Identification == oboznachenie).FirstOrDefault();
            if (in_in == null)
            {
                var buf_prd = product_in.Copy();
                buf_prd.Count = kolichestvo;
                base_product.Products.Add(buf_prd);
                base_product.Contaiment.Add(buf_prd.BaseId, buf_prd.Count);
            }
            else //потом посчитать кол-во общее вдруг где то используется много какой нибудь херни
                in_in.Count += kolichestvo;
        }
        public void SelectorSwitch(string text, int DeepLvl)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                Selector = new ObservableCollection<Product>(Productes.Where(t => t.DeepLevel <= (int)DeepLvl));
            }
            else
            {
                ObservableCollection<Product> a = new ObservableCollection<Product>(Productes.Where(t => t.ToXString.Contains(text) & t.DeepLevel <= (int)DeepLvl));
                Selector = a;
            }
        }
        #endregion

    }
}
