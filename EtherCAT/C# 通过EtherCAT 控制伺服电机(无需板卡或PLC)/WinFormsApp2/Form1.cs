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
            _etherCATMaster.StartActivity("��̫��");
            //_etherCATMaster1 =new EtherCATMaster();
            //_axis1 = new EtherCATSlave_CiA402_2(_etherCATMaster1, 1);
            //_etherCATMaster1.StartActivityEventHandler += _etherCATMaster1_StartActivityEventHandler;
            //_etherCATMaster1.StartActivity("��̫�� 5");// ˫����
        }

        private void _etherCATMaster1_StartActivityEventHandler(object? sender, EventArgs e)
        {
            _axis1?.Master.WriteSDO<byte>(_axis1.SlaveAddr, 0x6098, 0x00, 35);// ���û���ģʽ
            _axis1?.Master.WriteSDO<UInt32>(_axis1.SlaveAddr, 0x6099, 0x01, 8388608);// ԭ�����
            _axis1?.Master.WriteSDO<UInt32>(_axis1.SlaveAddr, 0x6099, 0x02, 8388608);// ԭ������
            _axis1?.Master.WriteSDO<UInt32>(_axis1.SlaveAddr, 0x609A, 0x00, 8388608 * 10);// ������ٶ�
        }

        private void _etherCATMaster_StartupEventHandler(object? sender, EventArgs e)
        {
            _axis.Master.WriteSDO<byte>(_axis.SlaveAddr, 0x6098, 0x00, 35);// ���û���ģʽ
            _axis.Master.WriteSDO<UInt32>(_axis.SlaveAddr, 0x6099, 0x01, 8388608);// ԭ�����
            _axis.Master.WriteSDO<UInt32>(_axis.SlaveAddr, 0x6099, 0x02, 8388608);// ԭ������
            _axis.Master.WriteSDO<UInt32>(_axis.SlaveAddr, 0x609A, 0x00, 8388608 * 10);// ������ٶ�
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
            _axis.MoveAbsolute(int.Parse(textBox1.Text), vel, vel * 10, vel * 10);// λ�� �ٶ� ���ٶ� ���ٶ�(��λ:Puls)
            _axis1?.MoveAbsolute(int.Parse(textBox1.Text), vel, vel * 10, vel * 10);
        }
        private void button5_Click(object sender, EventArgs e)
        {
            _axis.Stop(uint.Parse(textBox2.Text) * 10);// ֹͣ���ٶ�
            _axis1?.Stop(uint.Parse(textBox2.Text) * 10);// ֹͣ���ٶ�
        }
        private void button8_Click(object sender, EventArgs e)
        {
            _axis.Halt(uint.Parse(textBox2.Text) * 10);// ֹͣ���ٶ�
            _axis1?.Halt(uint.Parse(textBox2.Text) * 10);// ֹͣ���ٶ�
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            List<string> strings = new();
            strings.Add($"��վ��:{_axis.SlaveName}");
            strings.Add($"��վ��ַ:{_axis.SlaveAddr}");
            strings.Add($"��վ״̬:{_axis.SlaveState}");
            strings.Add($"��������:{_axis.ErrorCode}");
            strings.Add($"״̬��:{_axis.StatusWord}");
            strings.Add($"��ǰģʽ:{_axis.ModesOperationDisplay}");
            strings.Add($"��ǰλ��:{_axis.PositionActualValue}");
            strings.Add($"��ǰ�ٶ�:{_axis.VelocityActualValue}");
            strings.Add($"������:{_axis.Controlword}");
            strings.Add($"����ģʽ:{_axis.ModesOperation}");
            strings.Add($"����λ��:{_axis.TargetPosition}");
            strings.Add($"�����ٶ�:{_axis.ProfileVelocity}");
            strings.Add($"���ü��ٶ�:{_axis.ProfileAcceleration}");
            strings.Add($"���ü��ٶ�:{_axis.ProfileDeceleration}");
            // strings.Add($"����ͣ��:{_axis.QuickOptionCode}");
            // strings.Add($"ֹͣ����:{_axis.HaltOptionCode}");
            strings.Add($"�ŷ��޹���:{_axis.AxisState.ReadyToSwitchOn}");
            strings.Add($"�ȴ����ŷ�ʹ��:{_axis.AxisState.SwitchedOn}");
            strings.Add($"�ŷ�����:{_axis.AxisState.OperationEnabled}");
            strings.Add($"����:{_axis.AxisState.Fault}");
            strings.Add($"��ͨ����·��:{_axis.AxisState.VoltageEnabled}");
            strings.Add($"����ͣ��:{_axis.AxisState.QuickStop}");
            strings.Add($"�ŷ�׼����:{_axis.AxisState.SwitchOnDisabled}");
            strings.Add($"����:{_axis.AxisState.Warning}");
            strings.Add($"Զ�̿���:{_axis.AxisState.Remote}");
            strings.Add($"Ŀǰ����:{_axis.AxisState.TargetReached}");
            strings.Add($"�ڲ�����λ״̬:{_axis.AxisState.InternalLimitActive}");
            strings.Add($"��ԭ��������:{_axis.AxisState.HomingAttained}");

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
            _axis.MoveRelative(int.Parse(textBox1.Text), vel, vel * 10, vel * 10);// λ�� �ٶ� ���ٶ� ���ٶ�(��λ:Puls)
            _axis1?.MoveRelative(int.Parse(textBox1.Text), vel, vel * 10, vel * 10);
        }
    }
}
