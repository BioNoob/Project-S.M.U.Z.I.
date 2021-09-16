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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
#pragma warning disable CS0067

namespace Project_smuzi
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private DataBase _db;
        public DataBase DB
        {
            get { return _db; }
            set { _db = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DB")); }
        }
        public string Prefix { get; set; }

        public string FolderPath = string.Empty; //= Directory.GetParent(@"./").FullName;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            TB.Text = "ТСЮИ";
            DB = new DataBase();
            //Productes = new List<Product>();
            //Elementes = new List<Element>();

            if (!string.IsNullOrEmpty(Settings.Default.DB_json))
            {
                DB = JsonConvert.DeserializeObject<DataBase>(Settings.Default.DB_json);
                DB.LoadFromContaiment();
            }
            DB.Selector = DB.Productes;
            ComboBox_SelectionChanged(null, null);
            DB.ReadDataDone += DB_ReadDataDone;

        }

        private void DB_ReadDataDone()
        {
            this.Search_tb.IsEnabled = true;
            this.Deeb_cmb.IsEnabled = true;
            this.TB.IsEnabled = true;
            Deeb_cmb.SelectedIndex = DB.DeepList.Count - 1;
            //someControl.Dispatcher.Invoke(new Action(() => { /* Your code here */ }));
            //or Application.Current.Dispatcher.Invoke(new Action(() => { /* Your code here */ }));
            //RotateTransform rotateTransform = new RotateTransform(45);
            //while (true)
            //{
            //    Img_refresh.RenderTransform = rotateTransform;

            //}
        }

        public void TestSpwReader()
        {
            DB = new DataBase();
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
                            product.PathTo = item;
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
            DB.InvokeReadDataDone();
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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog ofd = new FolderBrowserDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FolderPath = ofd.SelectedPath;
                this.Search_tb.IsEnabled = false;
                this.Deeb_cmb.IsEnabled = false;
                this.TB.IsEnabled = false;
                await Task.Run(() => TestSpwReader());
            }
        }

        private void Search_tb_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if(Deeb_cmb.SelectedIndex < 0)
            {
                Deeb_cmb.SelectionChanged -= ComboBox_SelectionChanged;
                Deeb_cmb.SelectedIndex = 0;
                Deeb_cmb.SelectionChanged += ComboBox_SelectionChanged;
            }
            if (string.IsNullOrWhiteSpace(Search_tb.Text))
            {
                DB.Selector = new ObservableCollection<Product>(DB.Productes.Where(t => t.DeepLevel <= (int)Deeb_cmb.SelectedItem));
            }
            else
            {
                ObservableCollection<Product> a = new ObservableCollection<Product>(DB.Productes.Where(t => t.ToXString.Contains(Search_tb.Text) & t.DeepLevel <= (int)Deeb_cmb.SelectedItem));
                DB.Selector = a;
            }
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Search_tb_TextChanged(null, null);
        }

        private void TextBlock_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ContextMenu cm = FindResource("cmProduct") as ContextMenu;
            cm.PlacementTarget = sender as UIElement;
            cm.IsOpen = true;
            cm.Tag = ((TextBlock)sender).Text;

        }

        private void MenuItem_Folder_Click(object sender, RoutedEventArgs e)
        {
            var a = (Product)((MenuItem)sender).DataContext;
            Process.Start("explorer.exe", $"{a.FolderTo}");
        }
    }
}
