using Kompas6Constants;
using KompasAPI7;
using Project_smuzi.Classes;
using Project_smuzi.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
#pragma warning disable CS0067

namespace Project_smuzi
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public enum SearchMode
        {
            Base,
            Additional
        }

        public DataBase DB { get; set; }
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
                DB = JsonSerializer.Deserialize<DataBase>(Settings.Default.DB_json);
            treeView1.ItemsSource = DB.HeavyProducts;
        }

        public void TestSpwReader()
        {
            DB = new DataBase();
            IApplication kmpsApp = null;
            //string spFile = @"C:\Users\user\Documents\ТСЮИ.468359.109sp.spw";
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

                    //var seqtions = spec_descript.Active.SpecificationStyle.Sections; // СЕКЦИИ ДОКУМЕНТА
                    //sect name = qw[0].Name 
                    //sect ID = qw[0].Number

                    string name = "";
                    string ident = "";
                    if (kmpsdoc.LayoutSheets.Count > 0)
                    {
                        ident = kmpsdoc.LayoutSheets[0].Stamp.Text[0].Str.Trim();
                        name = kmpsdoc.LayoutSheets[0].Stamp.Text[1].Str.Trim();
                    }
                    //
                    if (!ident.Contains(Prefix))
                        ident = Path.GetFileNameWithoutExtension(item);
                    Product product = null;
                    var buf = DB.Productes.Where(t => t.Identification == ident).FirstOrDefault();
                    if (buf == null) //если нет в спсике изделий
                    {
                        product = new Product(ident) { Name = name, Count = 1, Section_id = 15 };
                        DB.Productes.Add(product); //добавляем изделие
                    }
                    else
                    {
                        product = buf;
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
                    Debug.WriteLine($"doc {ident} is {name} work done");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{ex.Message}\ndoc {item} open error");
                }

            }
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            Settings.Default.DB_json = JsonSerializer.Serialize(DB, options);
            Settings.Default.Save();
            Settings.Default.Reload();

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
            int kolichestvo = 0;
            int.TryParse(iSepcObj.Columns.Column[ksSpecificationColumnTypeEnum.ksSColumnCount, 1, 0].Text.Str, out kolichestvo);//   # кол-во   
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
            int kolichestvo = 0;
            int.TryParse(iSepcObj.Columns.Column[ksSpecificationColumnTypeEnum.ksSColumnCount, 1, 0].Text.Str, out kolichestvo);//  # кол-во   
            Placer(base_product, sect_num, naimenovanie, oboznachenie, kolichestvo);
        }
        private void Placer(Product base_product, int sect_num, string naimenovanie, string oboznachenie, int kolichestvo)
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
        private void AddElement(string oboznachenie, string naimenovanie, int section_num, int kolichestvo, Product base_product)
        {
            //ЕЛЕМЕНТ
            Element element_in = null;
            var buf_in = DB.Elementes.Where(t => t.Name == naimenovanie).FirstOrDefault();
            if (buf_in == null) //если нет в спсике елементов
            {
                element_in = new Element(oboznachenie) { Name = naimenovanie, Count = 1, Section_id = section_num };
                DB.Elementes.Add(element_in); //добавляем елемент
            }
            else
            {
                element_in = buf_in;
            }
            //Если есть такой элемент внутри издеия, то увеличиваем кол-во, если нет добавляем
            var in_in = base_product.Elements.Where(t => t.Name == naimenovanie).FirstOrDefault();
            if (in_in == null)
                base_product.Elements.Add(element_in);
            else
                in_in.Count += kolichestvo;
        }
        private void AddProduct(string oboznachenie, string naimenovanie, int section_num, int kolichestvo, Product base_product)
        {
            Product product_in = null;
            var buf_in = DB.Productes.Where(t => t.Identification == oboznachenie).FirstOrDefault();
            if (buf_in == null) //если нет в спсике изделий
            {
                product_in = new Product(oboznachenie) { Name = naimenovanie, Count = 1, Section_id = 15 };
                DB.Productes.Add(product_in); //добавляем изделие
            }
            else
            {
                product_in = buf_in;
            }
            //Если есть такое изделие внутри издеия, то увеличиваем кол-во, если нет добавляем
            var in_in = base_product.Products.Where(t => t.Identification == oboznachenie).FirstOrDefault();
            if (in_in == null)
                base_product.Products.Add(product_in);
            else
                in_in.Count += kolichestvo;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog ofd = new FolderBrowserDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FolderPath = ofd.SelectedPath;
                await Task.Run(()=> TestSpwReader());
            }
        }
    }
}
