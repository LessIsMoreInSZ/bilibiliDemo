using Newtonsoft.Json;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace functions
{
    public class TestViewModel : BindableBase
    {
        private readonly IDialogService _dialogService;
        private readonly IEventAggregator _aggregator;
        private string _languagePath = Path.Combine(AppContext.BaseDirectory, "Language");

        public TestViewModel()
        {
            LanguageInit();
        }

        public TestViewModel(IUnityContainer unityContainer, IRegionManager regionManager,
            IDialogService dialogService, IEventAggregator aggregator)
        {
            LanguageInit();
        }

        private void LanguageChange(string obj)
        {
            LanguageInit();
        }

        public void LanguageInit()
        {
            try
            {
                if (Directory.Exists(_languagePath))
                {
                    if (File.Exists(Path.Combine(_languagePath, "Data2CsvLanguage.json")))
                    {
                        string jsonStr = File.ReadAllText(Path.Combine(_languagePath, "Data2CsvLanguage.json"));
                        var language = JsonConvert.DeserializeObject<Data2CsvLanguage>(jsonStr)!;

                        Data2CsvLanguage.StartTime = ChangeLanguage(language.StartTime);
                        Data2CsvLanguage.EndTime = ChangeLanguage(language.EndTime);
                        Data2CsvLanguage.Select = ChangeLanguage(language.Select);
                        Data2CsvLanguage.SaveCsv = ChangeLanguage(language.SaveCsv);
                        Data2CsvLanguage.Flow = ChangeLanguage(language.Flow);
                        Data2CsvLanguage.Pressure = ChangeLanguage(language.Pressure);
                        Data2CsvLanguage.Vacuum = ChangeLanguage(language.Vacuum);
                        Data2CsvLanguage.Id = ChangeLanguage(language.Id);
                        Data2CsvLanguage.Start = language.Start?.Split(';').ToList()?.Where(l => l.Split('=')[0] == "中文").FirstOrDefault()?.Split('=')[1];

                        Data2CsvLanguage.Start = ChangeLanguage(language.Start);
                        Data2CsvLanguage.Scheme = ChangeLanguage(language.Scheme);
                        Data2CsvLanguage.Stop = ChangeLanguage(language.Stop);
                        Data2CsvLanguage.Zongchouqi = ChangeLanguage(language.Zongchouqi);
                        Data2CsvLanguage.Wurandu = ChangeLanguage(language.Wurandu);
                        Data2CsvLanguage.ProductTime = ChangeLanguage(language.ProductTime);
                        Data2CsvLanguage.Channel = ChangeLanguage(language.Channel);
                        Data2CsvLanguage.Min_pressure = ChangeLanguage(language.Min_pressure);
                        Data2CsvLanguage.Reaction_time = ChangeLanguage(language.Reaction_time);
                        Data2CsvLanguage.Air_time = ChangeLanguage(language.Air_time);
                        Data2CsvLanguage.Closed_position = ChangeLanguage(language.Closed_position);
                        Data2CsvLanguage.Speed = ChangeLanguage(language.Speed);
                        Data2CsvLanguage.Position = ChangeLanguage(language.Position);

                        // 反射可以实现出于性能考虑暂时不用
                        //SetPropertiesViaReflection(Data2CsvLanguage,language);

                    }
                    else
                    {
                        using (FileStream fs = new FileStream(Path.Combine(_languagePath, "Data2CsvLanguage.json"), FileMode.Create)) { }

                        File.WriteAllText(Path.Combine(_languagePath, "Data2CsvLanguage.json"), JsonConvert.SerializeObject(Data2CsvLanguage, Newtonsoft.Json.Formatting.Indented));
                    }
                }
                else
                {
                    Directory.CreateDirectory(_languagePath);

                    using (FileStream fs = new FileStream(Path.Combine(_languagePath, "Data2CsvLanguage.json"), FileMode.Create)) { }

                    File.WriteAllText(Path.Combine(_languagePath, "Data2CsvLanguage.json"), JsonConvert.SerializeObject(Data2CsvLanguage, Newtonsoft.Json.Formatting.Indented), new UTF8Encoding(false));
                }
            }
            catch (Exception ex)
            {
            }
        }

        private string ChangeLanguage(string? language)
        {
            return language.Split(';').ToList()?.Where(l => l.Split('=')[0] == "中文").FirstOrDefault()?.Split('=')[1];
        }



        private Data2CsvLanguage data2CsvLanguage = new Data2CsvLanguage();
        public Data2CsvLanguage Data2CsvLanguage
        {
            get { return data2CsvLanguage; }
            set { data2CsvLanguage = value; RaisePropertyChanged(); }
        }

        private string num1 = "1";
        public string Num1
        {
            get { return num1; }
            set { num1 = value; RaisePropertyChanged(); }
        }

        private string num2 = "2";
        public string Num2
        {
            get { return num2; }
            set { num2 = value; RaisePropertyChanged(); }
        }

        private string num3 = "3";
        public string Num3
        {
            get { return num3; }
            set { num3 = value; RaisePropertyChanged(); }
        }

        private string num4 = "4";
        public string Num4
        {
            get { return num4; }
            set { num4 = value; RaisePropertyChanged(); }
        }

        private string num5 = "5";
        public string Num5
        {
            get { return num5; }
            set { num5 = value; RaisePropertyChanged(); }
        }

        private string num6 = "6";
        public string Num6
        {
            get { return num6; }
            set { num6 = value; RaisePropertyChanged(); }
        }

        private string num7 = "7";
        public string Num7
        {
            get { return num7; }
            set { num7 = value; RaisePropertyChanged(); }
        }

        private string num8 = "8";
        public string Num8
        {
            get { return num8; }
            set { num8 = value; RaisePropertyChanged(); }
        }

        private DateTime startDate = DateTime.Now.AddDays(-1);
        public DateTime StartDate
        {
            get { return startDate; }
            set { startDate = value; RaisePropertyChanged(); }
        }

        private DateTime endDate = DateTime.Now;
        public DateTime EndDate
        {
            get { return endDate; }
            set { endDate = value; RaisePropertyChanged(); }
        }

        private bool isChanelUse1 = true;
        public bool IsChanelUse1
        {
            get { return isChanelUse1; }
            set { isChanelUse1 = value; RaisePropertyChanged(); }
        }

        private bool isChanelUse2 = true;
        public bool IsChanelUse2
        {
            get { return isChanelUse2; }
            set { isChanelUse2 = value; RaisePropertyChanged(); }
        }

        private bool isChanelUse3 = true;
        public bool IsChanelUse3
        {
            get { return isChanelUse3; }
            set { isChanelUse3 = value; RaisePropertyChanged(); }
        }

        private bool isChanelUse4 = true;
        public bool IsChanelUse4
        {
            get { return isChanelUse4; }
            set { isChanelUse4 = value; RaisePropertyChanged(); }
        }

        private bool isChanelUse5 = true;
        public bool IsChanelUse5
        {
            get { return isChanelUse5; }
            set { isChanelUse5 = value; RaisePropertyChanged(); }
        }

        private bool isChanelUse6 = true;
        public bool IsChanelUse6
        {
            get { return isChanelUse6; }
            set { isChanelUse6 = value; RaisePropertyChanged(); }
        }

        private bool isChanelUse7;
        public bool IsChanelUse7
        {
            get { return isChanelUse7; }
            set { isChanelUse7 = value; RaisePropertyChanged(); }
        }

        private bool isChanelUse8;
        public bool IsChanelUse8
        {
            get { return isChanelUse8; }
            set { isChanelUse8 = value; RaisePropertyChanged(); }
        }






        private DataTable AddColumns()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add(Data2CsvLanguage.Id);
            dataTable.Columns.Add(Data2CsvLanguage.Scheme);
            dataTable.Columns.Add(Data2CsvLanguage.Start);
            dataTable.Columns.Add(Data2CsvLanguage.Stop);
            dataTable.Columns.Add(Data2CsvLanguage.Air_time);

            AddColumn(IsChanelUse1, dataTable, "1");
            AddColumn(IsChanelUse2, dataTable, "2");
            AddColumn(IsChanelUse3, dataTable, "3");
            AddColumn(IsChanelUse4, dataTable, "4");
            AddColumn(IsChanelUse5, dataTable, "5");
            AddColumn(IsChanelUse6, dataTable, "6");
            AddColumn(IsChanelUse7, dataTable, "7");
            AddColumn(IsChanelUse8, dataTable, "8");
            return dataTable;
        }

        private void AddColumn(bool isChanel, DataTable dataTable, string num)
        {
            if (isChanel)
            {
                dataTable.Columns.Add(Data2CsvLanguage.Channel + num);
                dataTable.Columns.Add(Data2CsvLanguage.Min_pressure + num);
                dataTable.Columns.Add(Data2CsvLanguage.Reaction_time + num);
                dataTable.Columns.Add(Data2CsvLanguage.Air_time + num);
                dataTable.Columns.Add(Data2CsvLanguage.Closed_position + num);
                dataTable.Columns.Add(Data2CsvLanguage.Wurandu + num);
            }
        }

    }
}
