using Core;
using Core.Net.EtherCAT;
using Core.Net.EtherCAT.SeedWork;
using Core.Net.EtherCAT.SeedWork.Interrop;
using System;
using System.Diagnostics;
using System.Runtime.Intrinsics;
using System.Windows.Forms;

namespace WinFormsApp2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        EtherCATMaster? _etherCATMaster;
        EtherCATMaster? _etherCATMaster1;
        IEtherCATSlave_CiA402 _axis;
        IEtherCATSlave_CiA402? _axis1;
        private void Form1_Load(object sender, EventArgs e)
        {
            _etherCATMaster = new EtherCATMaster();
            _axis = new EtherCATSlave_CiA402_2(_etherCATMaster, 1);
            //  _axis1 = new EtherCATSlave_CiA402_2(_etherCATMaster, 1, offset: 0x800, txcontent: 0x1A10, rxcontent: 0x1610) { SyncFrequency = 10 };
            //  _etherCATMaster.StartupEventHandler += _etherCATMaster_StartupEventHandler;
            _etherCATMaster.StartActivity("以太网");
            //_etherCATMaster1 =new EtherCATMaster();
            //_axis1 = new EtherCATSlave_CiA402_2(_etherCATMaster1, 1);
            //_etherCATMaster1.StartActivityEventHandler += _etherCATMaster1_StartActivityEventHandler;
            //_etherCATMaster1.StartActivity("以太网 5");// 双网口
        }

        private void _etherCATMaster1_StartActivityEventHandler(object? sender, EventArgs e)
        {
            _axis1?.Master.WriteSDO<byte>(_axis1.SlaveAddr, 0x6098, 0x00, 35);// 设置回零模式
            _axis1?.Master.WriteSDO<UInt32>(_axis1.SlaveAddr, 0x6099, 0x01, 8388608);// 原点快速
            _axis1?.Master.WriteSDO<UInt32>(_axis1.SlaveAddr, 0x6099, 0x02, 8388608);// 原点慢速
            _axis1?.Master.WriteSDO<UInt32>(_axis1.SlaveAddr, 0x609A, 0x00, 8388608 * 10);// 回零加速度
        }

        private void _etherCATMaster_StartupEventHandler(object? sender, EventArgs e)
        {
            _axis.Master.WriteSDO<byte>(_axis.SlaveAddr, 0x6098, 0x00, 35);// 设置回零模式
            _axis.Master.WriteSDO<UInt32>(_axis.SlaveAddr, 0x6099, 0x01, 8388608);// 原点快速
            _axis.Master.WriteSDO<UInt32>(_axis.SlaveAddr, 0x6099, 0x02, 8388608);// 原点慢速
            _axis.Master.WriteSDO<UInt32>(_axis.SlaveAddr, 0x609A, 0x00, 8388608 * 10);// 回零加速度
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _axis.Reset();
            _axis1?.Reset();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            _axis.PowerOn();
            _axis1?.PowerOn();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            _axis.PowerOff();
            _axis1?.PowerOff();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            var vel = uint.Parse(textBox2.Text);
            _axis.MoveAbsolute(int.Parse(textBox1.Text), vel, vel * 10, vel * 10);// 位置 速度 加速度 减速度(单位:Puls)
            _axis1?.MoveAbsolute(int.Parse(textBox1.Text), vel, vel * 10, vel * 10);
        }
        private void button5_Click(object sender, EventArgs e)
        {
            _axis.Stop(uint.Parse(textBox2.Text) * 10);// 停止减速度
            _axis1?.Stop(uint.Parse(textBox2.Text) * 10);// 停止减速度
        }
        private void button8_Click(object sender, EventArgs e)
        {
            _axis.Halt(uint.Parse(textBox2.Text) * 10);// 停止减速度
            _axis1?.Halt(uint.Parse(textBox2.Text) * 10);// 停止减速度
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            List<string> strings = new();
            strings.Add($"从站名:{_axis.SlaveName}");
            strings.Add($"从站地址:{_axis.SlaveAddr}");
            strings.Add($"从站状态:{_axis.SlaveState}");
            strings.Add($"报警代码:{_axis.ErrorCode}");
            strings.Add($"状态字:{_axis.StatusWord}");
            strings.Add($"当前模式:{_axis.ModesOperationDisplay}");
            strings.Add($"当前位置:{_axis.PositionActualValue}");
            strings.Add($"当前速度:{_axis.VelocityActualValue}");
            strings.Add($"控制字:{_axis.Controlword}");
            strings.Add($"设置模式:{_axis.ModesOperation}");
            strings.Add($"设置位置:{_axis.TargetPosition}");
            strings.Add($"设置速度:{_axis.ProfileVelocity}");
            strings.Add($"设置加速度:{_axis.ProfileAcceleration}");
            strings.Add($"设置减速度:{_axis.ProfileDeceleration}");
            // strings.Add($"快速停机:{_axis.QuickOptionCode}");
            // strings.Add($"停止代码:{_axis.HaltOptionCode}");
            strings.Add($"伺服无故障:{_axis.AxisState.ReadyToSwitchOn}");
            strings.Add($"等待打开伺服使能:{_axis.AxisState.SwitchedOn}");
            strings.Add($"伺服运行:{_axis.AxisState.OperationEnabled}");
            strings.Add($"故障:{_axis.AxisState.Fault}");
            strings.Add($"接通主回路电:{_axis.AxisState.VoltageEnabled}");
            strings.Add($"快速停机:{_axis.AxisState.QuickStop}");
            strings.Add($"伺服准备好:{_axis.AxisState.SwitchOnDisabled}");
            strings.Add($"警告:{_axis.AxisState.Warning}");
            strings.Add($"远程控制:{_axis.AxisState.Remote}");
            strings.Add($"目前到达:{_axis.AxisState.TargetReached}");
            strings.Add($"内部软限位状态:{_axis.AxisState.InternalLimitActive}");
            strings.Add($"回原点完成输出:{_axis.AxisState.HomingAttained}");

            listBox1.Items.Clear();
            listBox1.Items.AddRange(strings.ToArray());
        }
        private void button6_Click(object sender, EventArgs e)
        {
            _axis.Home();
            _axis1?.Home();
        }
        private void button7_Click(object sender, EventArgs e)
        {
            var vel = uint.Parse(textBox2.Text);
            _axis.MoveRelative(int.Parse(textBox1.Text), vel, vel * 10, vel * 10);// 位置 速度 加速度 减速度(单位:Puls)
            _axis1?.MoveRelative(int.Parse(textBox1.Text), vel, vel * 10, vel * 10);
        }
    }
}
