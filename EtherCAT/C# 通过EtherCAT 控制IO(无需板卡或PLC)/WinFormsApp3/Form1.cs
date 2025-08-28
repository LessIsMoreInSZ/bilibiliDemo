using Core;
using Core.Net.EtherCAT;
using Core.Net.EtherCAT.SeedWork;
using System.Drawing.Imaging;

namespace WinFormsApp3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        EtherCATMaster _etherCATMaster;
        IEtherCATSlave _etherCATSlave;
        ReceivePDOMapping out1;
        ReceivePDOMapping out2;
        TransmitPDOMapping in1;
        TransmitPDOMapping in2;
        private void Form1_Load(object sender, EventArgs e)
        {
            _etherCATMaster = new EtherCATMaster();
            _etherCATSlave = new EtherCATSlave(_etherCATMaster, 2);// �㴨 GL10_RTU_ECTA(Ӳ��������16������ ��16�����)
            //// ���� GL10_1600END
            //in1 = _etherCATSlave.AddTxPDOMapping(0x1A00, 0x6101, 0x01, typeof(byte));
            //in2 = _etherCATSlave.AddTxPDOMapping(0x1A00, 0x6101, 0x02, typeof(byte));
            //// ��� GL10_0016ETN_ETP_ER_
            //out1 = _etherCATSlave.AddRxPDOMapping(0x1610, 0x7002, 0x01, typeof(byte));
            //out2 = _etherCATSlave.AddRxPDOMapping(0x1610, 0x7002, 0x02, typeof(byte));
            //in1 = _etherCATSlave.AddTxPDOMapping(0, typeof(byte));// Inputs���� ����0
            //in2 = _etherCATSlave.AddTxPDOMapping(1, typeof(byte));
            //out1 = _etherCATSlave.AddRxPDOMapping(0, typeof(byte));// Ouptus���� ����0
            //out2 = _etherCATSlave.AddRxPDOMapping(1, typeof(byte));

            _etherCATMaster.StartActivity("��̫��");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = $"{_etherCATSlave.Inputs?[0] ?? null}";// in1.Value?.ToString();
            textBox2.Text = $"{_etherCATSlave.Inputs?[1] ?? null}";// in2.Value?.ToString();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if(_etherCATSlave.Outputs is not null && _etherCATSlave.Outputs.Length>=2)
            {
                //out1.Value = byte.Parse(textBox3.Text);
                //out2.Value = byte.Parse(textBox4.Text);
                _etherCATSlave.Outputs[0] = byte.Parse(textBox3.Text);
                _etherCATSlave.Outputs[1] = byte.Parse(textBox3.Text);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            List<string> strings = new();
            strings.Add($"��վ��:{_etherCATSlave.SlaveName}");
            strings.Add($"��վ��ַ:{_etherCATSlave.SlaveAddr}");
            strings.Add($"��վ״̬:{_etherCATSlave.SlaveState}");
            strings.Add($"��վ���õ�ַ:{_etherCATSlave.SlaveEctAdr.ToString("X")}");
            listBox1.Items.Clear();
            listBox1.Items.AddRange(strings.ToArray());

            textBox1.Text = $"{_etherCATSlave.Inputs?[0] ?? null}";// in1.Value?.ToString();
            textBox2.Text = $"{_etherCATSlave.Inputs?[1] ?? null}";// in2.Value?.ToString();
        }
    }
}
