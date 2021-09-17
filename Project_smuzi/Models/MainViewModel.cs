using Kompas6Constants;
using KompasAPI7;
using Newtonsoft.Json;
using Project_smuzi.Classes;
using Project_smuzi.Properties;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project_smuzi.Models
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            DB = new DataBase();

            if (!string.IsNullOrEmpty(Settings.Default.DB_json))
            {
                DB = JsonConvert.DeserializeObject<DataBase>(Settings.Default.DB_json);
                DB.LoadFromContaiment();

            }
            DB.Selector = DB.Productes;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private DataBase _db;
        public DataBase DB
        {
            get { return _db; }
            set { _db = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DB")); }
        }
        public string Prefix { get; set; }
        public void TestSpwReader(string FolderPath)
        {
            DB.Clear();
            //DB.ReadDataDone += DB_ReadDataDone;
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
                            ident = Path.GetFileNameWithoutExtension(item);
                    }

                    Product product = null;
                    var buf = DB.Productes.Where(t => t.Identification == ident).FirstOrDefault();
                    if (buf == null) //если нет в спсике изделий
                    {
                        product = new Product(ident) { Name = name, Count = 1, Section_id = 15, DeepLevel = 0, PathTo = item };
                        DB.Productes.Add(product); //добавляем изделие
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
                                DB.Productes.Where(t => t.BaseId == prd).FirstOrDefault().Products.Where(t => t.BaseId == product.BaseId).FirstOrDefault().PathTo = item;
                            }
                        }
                    }

                    var iSpecificationBaseObjects = spec_descript.Active.BaseObjects;
                    for (int i = 0; i < iSpecificationBaseObjects.Count; i++)
                    {
                        InnerVorker_Base(iSpecificationBaseObjects[i], product);
                    }
                    var iSpecificationCommentObjects = spec_descript.Active.CommentObjects;
                    for (int i = 0; i < iSpecificationCommentObjects.Count; i++)
                    {
                        InnerVorker_Additioanl(iSpecificationCommentObjects[i], product);
                    }

                    kmpsdoc.Close(DocumentCloseOptions.kdDoNotSaveChanges);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DB"));
                    DB.Selector = DB.HeavyProducts;
                    Debug.WriteLine($"doc {ident} is {name} work done");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{ex.Message}\ndoc {item} open error");
                }
            }
            Settings.Default.DB_json = JsonConvert.SerializeObject(DB, Formatting.Indented);
            Settings.Default.Save();
            Settings.Default.Reload();
            Debug.WriteLine($"Save and done {all_dir.Count} documents");
            InvokeReadDataDone();
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
            var buf_in = DB.Elementes.Where(t => t.Name == naimenovanie).FirstOrDefault();
            if (buf_in == null) //если нет в спсике елементов
            {
                element_in = new Element(oboznachenie) { Name = naimenovanie, Count = kolichestvo, Section_id = section_num };
                DB.Elementes.Add(element_in); //добавляем елемент
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
            var buf_in = DB.Productes.Where(t => t.Identification == oboznachenie).FirstOrDefault();
            if (buf_in == null) //если нет в спсике изделий
            {
                product_in = new Product(oboznachenie) { Name = naimenovanie, Count = kolichestvo, Section_id = section_num };
                DB.Productes.Add(product_in); //добавляем изделие
            }
            else
            {
                product_in = buf_in;
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
                DB.Selector = new ObservableCollection<Product>(DB.Productes.Where(t => t.DeepLevel <= (int)DeepLvl));
            }
            else
            {
                ObservableCollection<Product> a = new ObservableCollection<Product>(DB.Productes.Where(t => t.ToXString.Contains(text) & t.DeepLevel <= (int)DeepLvl));
                DB.Selector = a;
            }
        }

        private CommandHandler _startread;
        public CommandHandler StartReadCommand
        {
            get
            {
                return _startread ?? (_startread = new CommandHandler(async obj =>
                {
                    FolderBrowserDialog ofd = new FolderBrowserDialog();
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        InvokeReadDataStart();
                        await Task.Run(() => TestSpwReader(ofd.SelectedPath));
                    }
                }//,
                //(obj) => string.IsNullOrEmpty(obj.ToString())
                ));
            }
        }

        public delegate void JobInfo();
        public event JobInfo ReadDataDone;
        public event JobInfo ReadDataStart;

        private void InvokeReadDataDone()
        {
            ReadDataDone?.Invoke();
        }
        private void InvokeReadDataStart()
        {
            ReadDataStart?.Invoke();
        }
    }
}
