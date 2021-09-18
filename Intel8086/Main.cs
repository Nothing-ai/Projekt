using System;
using System.Windows.Forms;
using static Intel8086.Instruction;
using static Intel8086.RegisterLetter;
using static Intel8086.RegisterType;
using static Intel8086.Register;

namespace Intel8086
{
    public partial class Main : Form
    {
        private Instruction _instruction = Mov;
        private Register _source = new() { Letter = A, Type = X }; 
        private Register _destination = new() { Letter = B, Type = X };
        private bool _updating = true;
        public Main()
        {
            InitializeComponent();
            DisplayRegisters();
        }

        private void SetSourceFromCheck(object sender, EventArgs e)
        {
            if (sender is not RadioButton {Checked: true, Text: var registerName}) return;
            _source = FromName(registerName);
        }
        private void SetDestinationFromCheck(object sender, EventArgs e)
        {
            if (sender is not RadioButton {Checked: true, Text: var registerName}) return;
            _destination = FromName(registerName);
        }

        private void SetInstructionFromCheck(object sender, EventArgs e)
        {
            if (sender is not RadioButton {Checked:true} radioButton) return;
            var text = radioButton.Text;
            _instruction = Enum.Parse<Instruction>($"{char.ToUpper(text[0])}{text[1..].ToLower()}");
            switch (_instruction)
            {
                case Mov:
                case Xchg:
                    sourceGroup.Enabled = true;
                    destinationGroup.Enabled = true;
                    break;
                case Pop:
                    sourceGroup.Enabled = false;
                    destinationGroup.Enabled = true;
                    break;
                case Push:
                    sourceGroup.Enabled = true;
                    destinationGroup.Enabled = false;
                    break;
            }
        }

        private void DisplayRegisters()
        {
            _updating = true;
            AX.Text = Cpu.Read(Register.AX).ToString("X4");
            BX.Text = Cpu.Read(Register.BX).ToString("X4");
            CX.Text = Cpu.Read(Register.CX).ToString("X4");
            DX.Text = Cpu.Read(Register.DX).ToString("X4");

            AL.Text = Cpu.Read(Register.AL).ToString("X2");
            BL.Text = Cpu.Read(Register.BL).ToString("X2");
            CL.Text = Cpu.Read(Register.CL).ToString("X2");
            DL.Text = Cpu.Read(Register.DL).ToString("X2");

            AH.Text = Cpu.Read(Register.AH).ToString("X2");
            BH.Text = Cpu.Read(Register.BH).ToString("X2");
            CH.Text = Cpu.Read(Register.CH).ToString("X2");
            DH.Text = Cpu.Read(Register.DH).ToString("X2");


            stack.Items.Clear();
            foreach (var entry in Cpu.Stack)
            {
                stack.Items.Add(entry.ToString("X4"));
            }

            _updating = false;
        }

        private void exeButton_Click(object sender, EventArgs e)
        {
            switch (_instruction)
            {
                case Mov:
                    Cpu.Mov(_source, _destination);
                    break;
                case Push:
                    Cpu.Push(_source);
                    break;
                case Pop:
                    Cpu.Pop(_destination);
                    break;
                case Xchg:
                    Cpu.Xchg(_source, _destination);
                    break;
            }
            DisplayRegisters();
        }

        private void RegisterValueChanged(object sender, EventArgs e)
        {
            if (_updating || sender is not MaskedTextBox {Mask: {Length: var maskLength}, Text: var text, Tag: string tag} || text.Length != maskLength) return;
            try
            {
                var newValue = Convert.ToInt32(text, 16);
                var register = Register.FromName(tag);
                Cpu.Store(register, newValue);
            }
            catch
            {
                MessageBox.Show($"Error: {text} is invalid value for register {tag}");
            }
            DisplayRegisters();
        }
    }
}
