using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace functions
{
    public class Data2CsvLanguage : BindableBase
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        private string? startTime = "开始时间";
        public string? StartTime
        {
            get { return startTime; }
            set { startTime = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 结束时间
        /// </summary>
        private string? endTime;
        public string? EndTime
        {
            get { return endTime; }
            set { endTime = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 曲线选择
        /// </summary>
        private string? select;
        public string? Select
        {
            get { return select; }
            set { select = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 保存为csv
        /// </summary>
        private string? saveCsv;
        public string? SaveCsv
        {
            get { return saveCsv; }
            set { saveCsv = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 流量
        /// </summary>
        private string? flow;
        public string? Flow
        {
            get { return flow; }
            set { flow = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 压力
        /// </summary>
        private string? pressure;
        public string? Pressure
        {
            get { return pressure; }
            set { pressure = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 真空
        /// </summary>
        private string? vacuum;
        public string? Vacuum
        {
            get { return vacuum; }
            set { vacuum = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 产品ID
        /// </summary>
        private string? id = "产品ID";
        public string? Id
        {
            get { return id; }
            set { id = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 起始位置
        /// </summary>
        private string? start;
        public string? Start
        {
            get { return start; }
            set { start = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 方案名称
        /// </summary>
        private string? scheme;
        public string? Scheme
        {
            get { return scheme; }
            set { scheme = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 结束位置
        /// </summary>
        private string? stop;
        public string? Stop
        {
            get { return stop; }
            set { stop = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 总抽气
        /// </summary>
        private string? zongchouqi;
        public string? Zongchouqi
        {
            get { return zongchouqi; }
            set { zongchouqi = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 污染度
        /// </summary>
        private string? wurandu;
        public string? Wurandu
        {
            get { return wurandu; }
            set { wurandu = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 生产时间
        /// </summary>
        private string? productTime;
        public string? ProductTime
        {
            get { return productTime; }
            set { productTime = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 通道
        /// </summary>
        private string? channel;
        public string? Channel
        {
            get { return channel; }
            set { channel = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 最小压力
        /// </summary>
        private string? min_pressure;
        public string? Min_pressure
        {
            get { return min_pressure; }
            set { min_pressure = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 反应时间
        /// </summary>
        private string? reaction_time;
        public string? Reaction_time
        {
            get { return reaction_time; }
            set { reaction_time = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 通风时间
        /// </summary>
        private string? air_time;
        public string? Air_time
        {
            get { return air_time; }
            set { air_time = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 封闭位置
        /// </summary>
        private string? closed_position;
        public string? Closed_position
        {
            get { return closed_position; }
            set { closed_position = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 速度
        /// </summary>
        private string? speed;
        public string? Speed
        {
            get { return speed; }
            set { speed = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 位置
        /// </summary>
        private string? position;
        public string? Position
        {
            get { return position; }
            set { position = value; RaisePropertyChanged(); }
        }
    }
}
