using Kompas6Constants;
using KompasAPI7;
using Project_smuzi.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
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
        public List<Product> Products { get; set; }
        public List<Element> Elements { get; set; }
        public string Prefix { get; set; }

        public string FolderPath = string.Empty; //= Directory.GetParent(@"./").FullName;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            TB.Text = "ТСЮИ";
            Products = new List<Product>();
            Elements = new List<Element>();
        }

        public void TestSpwReader()
        {
            IApplication kmpsApp = null;
            //string spFile = @"C:\Users\user\Documents\ТСЮИ.468359.109sp.spw";
            Type t7 = Type.GetTypeFromProgID("KOMPAS.Application.7");
            kmpsApp = (IApplication)Activator.CreateInstance(t7);
            if (kmpsApp == null)
            {
                Console.Write("ERROR");
                return;
            }
            kmpsApp.HideMessage = ksHideMessageEnum.ksHideMessageYes;

            var all_dir = Directory.GetFiles(FolderPath, "*sp.spw*", SearchOption.AllDirectories).ToList();

            foreach (var item in all_dir)
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
                Product product = null;
                var buf = Products.Where(t => t.Identification == ident).FirstOrDefault();
                if (buf == null) //если нет в спсике изделий
                {
                    product = new Product(ident) { Name = name, Count = 1, Section_id = 15 };
                    Products.Add(product); //добавляем изделие
                }
                else
                {
                    product = buf;
                }



                var iSpecificationBaseObjects = spec_descript.Active.BaseObjects;
                for (int i = 0; i < iSpecificationBaseObjects.Count; i++)
                {
                    var aw = iSpecificationBaseObjects[i];
                    var sect_num = aw.Section;//aw.Section; // id секции в которой находится
                    if (sect_num == 0)
                        sect_num = aw.AdditionalSection;
                    var naimenovanie = aw.Columns.Column[ksSpecificationColumnTypeEnum.ksSColumnName, 1, 0].Text.Str.Trim();//   # 5 - колонка "Наименование"
                    var oboznachenie = aw.Columns.Column[ksSpecificationColumnTypeEnum.ksSColumnMark, 1, 0].Text.Str.Trim();//   # Обозначение
                    int kolichestvo = 0;
                    int.TryParse(aw.Columns.Column[ksSpecificationColumnTypeEnum.ksSColumnCount, 1, 0].Text.Str, out kolichestvo);//   # кол-во   

                    

                    //если соответствует префиксу = изделие.
                    //Ищем в списке изделий.
                    //Если есть то берем то что нашли и пихаем в текущее изделие в его изделия.
                    //Если нет, создаем изделие, пихаем в спсиок издейли, и в текущее в его изделия.
                    if (oboznachenie.Contains(Prefix))//item.Contains(Prefix))
                    {
                        if ("Документация" != Element.GetInterpritation(sect_num))
                            AddProduct(oboznachenie, naimenovanie, sect_num, kolichestvo, product);
                        else
                            AddElement(oboznachenie, naimenovanie, sect_num, kolichestvo, product);

                        ///!!!!!!!!!!!!!!!!!! происходит херня если нету идентификатора! Смотреть по имени!
                    }
                    else
                        AddElement(oboznachenie, naimenovanie, sect_num, kolichestvo, product);


                }
                kmpsdoc.Close(DocumentCloseOptions.kdDoNotSaveChanges);
            }
            kmpsApp.Quit();
        }

        private void AddElement(string oboznachenie, string naimenovanie, int section_num, int kolichestvo, Product base_product)
        {
            //ЕЛЕМЕНТ
            Element element_in = null;
            var buf_in = Elements.Where(t => t.Identification == oboznachenie).FirstOrDefault();
            if (buf_in == null) //если нет в спсике елементов
            {
                element_in = new Element(oboznachenie) { Name = naimenovanie, Count = 1, Section_id = section_num };
                Elements.Add(element_in); //добавляем елемент
            }
            else
            {
                element_in = buf_in;
            }
            //Если есть такой элемент внутри издеия, то увеличиваем кол-во, если нет добавляем
            var in_in = base_product.Elements.Where(t => t.Identification == oboznachenie).FirstOrDefault();
            if (in_in == null)
                base_product.Elements.Add(element_in);
            else
                in_in.Count += kolichestvo;
        }
        private void AddProduct(string oboznachenie, string naimenovanie, int section_num, int kolichestvo, Product base_product)
        {
            Product product_in = null;
            var buf_in = Products.Where(t => t.Identification == oboznachenie).FirstOrDefault();
            if (buf_in == null) //если нет в спсике изделий
            {
                product_in = new Product(oboznachenie) { Name = naimenovanie, Count = 1, Section_id = 15 };
                Products.Add(product_in); //добавляем изделие
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog ofd = new FolderBrowserDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FolderPath = ofd.SelectedPath;
                TestSpwReader();
            }
        }
    }
}
