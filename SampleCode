using Prism.Commands;
using Prism.Mvvm;
using SahraShoesMaterial.KafiService;
using SahraShoesMaterial.ModelService;
using SahraShoesMaterial.OtherService;
using SahraShoesMaterial.RuyeService;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SahraShoesMaterial.Modeling.Outworking
{
    /// <summary>
    /// Interaction logic for MainOutworking.xaml
    /// </summary>
    public partial class MainOutworking : UserControl
    {
        public MainOutworking()
        {
            InitializeComponent();
        }
    }

    public class OutworkingViewModel : BindableBase
    {
        public IEnumerable<ModTemplates> Templates { get; set; }
        private ModTemplates template;
        public ModTemplates Template { get => template; set => SetProperty(ref template, value); }
        ModelsClient modCl = new ModelsClient();

        private int modelingCode;
        public int ModelingCode { get => modelingCode; set => SetProperty(ref modelingCode, value); }

        private ModOutworks outwork;
        public ModOutworks Outwork { get => outwork; set => SetProperty(ref outwork, value); }

        private bool isComplete;
        public bool IsComplete { get => isComplete; set => SetProperty(ref isComplete, value); }

        public DelegateCommand SubmitCommand { get; set; }
        public DelegateCommand ReceiveCommand { get; set; }

        private string modelName;
        public string ModelName { get => modelName; set => SetProperty(ref modelName, value); }

        private string modelCode;
        public string ModelCode { get => modelCode; set => SetProperty(ref modelCode, value); }

        KafiClient kafiCl = new KafiClient();
        RuyeClient ruyeCl = new RuyeClient();
        UsersClient other = new UsersClient();

        public bool IsMachine { get; set; }

        public OutworkingViewModel()
        {
            Templates = modCl.GetTemplates().Where(a => a.Status == (byte)SharedLib.IEnums.TemplateStatus.SENDTOOUTWORK);
            PropertyChanged += OutworkingViewModel_PropertyChanged;
            SubmitCommand = new DelegateCommand(Submit);
            ReceiveCommand = new DelegateCommand(Receive);
        }

        private void Receive()
        {
            Outwork.ReceiveDate = DateTime.Now;
            Outwork.Status = (byte)IEnums.NormalStatus.Receive;
            Template.Status = (byte)SharedLib.IEnums.TemplateStatus.RECEIVEFROMOUTWORK;
            modCl.UpdateTemplate(Template);
            modCl.UpdateModOutwork(Outwork);
            MessageBox.Show("بروزرسانی با موفقیت انجام شد", "هشدار", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            Utils.Util.SetContentContainer(new MainOutworking(), null);
        }

        private void Submit()
        {
            Outwork.ReceiveDate = DateTime.Now;
            Outwork.Status = (byte)IEnums.NormalStatus.Receive;
            Template.Status = (byte)SharedLib.IEnums.TemplateStatus.COMPLETE;
            Template.ModelCode = ModelCode;
            Template.ModelName = ModelName;
            // 1. Check Model Code
            if(ModelCode.Length != 6)
            {
                MessageBox.Show("کد مدل درست انتخاب نشده است", "هشدار", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            if(modCl.getModels().Any(a => a.Code == ModelCode))
            {
                MessageBox.Show("کد مدل از قبل موجود است", "هشدار", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }

            // 2. Check Kafi
            IEnumerable<Kafi> KafiList = kafiCl.getSimpleKafiList();
            List<ModSections> Sections = modCl.GetTemplateSections(Template.Id).ToList();
            Kafi kafi;
            Zire zire;
            KafiZire kz;

            zire = kafiCl.getZireList().Single(a => a.Code == ModelCode.Substring(0, 2));
            if (KafiList.Any(a => a.Code == Template.ModelCode.Substring(0, 4)))
            {
                kafi = KafiList.Single(a => a.Code == Template.ModelCode.Substring(0, 4));
                kz = kafiCl.GetKafiZire().Single(a => a.Code == ModelCode.Substring(0, 4));
            }
            else
            {
                

                List<ModSections> kafiSections = Sections.Where(a => a.Section == 'K').ToList();
                kafi = new Kafi
                {
                    Code = Template.ModelCode.Substring(0, 4),
                    IsMachine = IsMachine ? (byte)1 : (byte)0,
                    Sections = (byte)kafiSections.Count(),
                    ZireId = zire.Id
                };
                kafi.Id = kafiCl.insertKafi(kafi);

                kz = new KafiZire
                {
                    Code = ModelCode.Substring(0, 4),
                    KafiId = kafi.Id,
                    ZireId = zire.Id
                };
                kz.Id = kafiCl.insertKafiZire(kz);


                for(int i =0; i < kafiSections.Count(); i++)
                {
                    ModSections ms = kafiSections[i];
                    KafiSections ks = new KafiSections
                    {
                        KafiId = kafi.Id,
                        Name = ms.SectionName,
                        Row = (byte)(i + 1),
                        Case = 1,
                        IsMain = 1,
                        ChangeRate = ms.ChangeRate
                    };

                    kafiCl.insertkafiSection(ks);
                }
            }

            // 2. Check Ruye
            List<ModSections> ruyeSections = Sections.Where(a => a.Section == 'R').ToList();
            Ruye ruye = new Ruye
            {
                Code = ModelCode,
                IsMachine = IsMachine ? (byte)1 : (byte)0,
                LabType = 1,
                Sections = (byte)ruyeSections.Count(),
            };

            ruye.Id = ruyeCl.insertRuye(ruye);
            List<RuyeSections> rSections = new List<RuyeSections>();
            for(int i =0; i< ruyeSections.Count(); i++)
            {
                ModSections ms = ruyeSections[i];
                RuyeSections rs = new RuyeSections
                {
                    Case = 1,
                    ChangeRate = ms.ChangeRate,
                    IsAstar = 0,
                    Name = ms.SectionName,
                    Row = (byte)(i + 1),
                    RuyeId = ruye.Id
                };

                rSections.Add(rs);
            }
            ruyeCl.insertSections(rSections.ToArray());

            // 3. Insert Model
            Stuff c = new Stuff
            {
                Type = 15,
                Code = "0",
                Value = ModelName
            };
            int NameId = other.InsertStuff(c);
            Models model = new Models
            {
                Code = ModelCode,
                NameId = NameId,
                Type = IsMachine ? (byte)IEnums.ProductType.MachinePrimary : (byte)IEnums.ProductType.ManualPrimary,
            };
            int modelId = modCl.insertModel(model);

            PrimaryDirectModels pdm = new PrimaryDirectModels
            {
                Code = ModelCode,
                KafiId = kafi.Id,
                ZireId = zire.Id,
                ModelId = modelId,
                RuyeId = ruye.Id,
                LabType = 1
            };

            modCl.insertPrimary(pdm);

            modCl.UpdateTemplate(Template);
            modCl.UpdateModOutwork(Outwork);
            MessageBox.Show("ثبت و ایجاد مدل جدید با موفقیت انجام شد", "پیغام", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            Utils.Util.SetContentContainer(new MainOutworking(), null);
        }

        private void OutworkingViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Template"))
            {
                if(Template != null)
                {
                    ModelingCode = Template.Id;
                    Outwork = modCl.GetTemplateOutworks().Single(a => a.TemplateId == Template.Id && a.Status == (byte)IEnums.NormalStatus.Sent);
                    ModelName = Template.ModelName;
                    ModelCode = Template.ModelCode;
                }
            }
        }
    }

    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool val = bool.Parse(value.ToString());
            return val ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
