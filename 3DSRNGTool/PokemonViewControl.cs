namespace Pk3DSRNGTool
{
    using System.Drawing;
    using System.Windows.Forms;

    public partial class PokemonViewControl : UserControl
    {
        public PokemonViewControl()
        {
            InitializeComponent();
        }

        public string Title { get => GB_Party.Text; set => GB_Party.Text = value; }

        public string Species { get => L_Species.Text; set => L_Species.Text = value; }
        public string Gender { get => L_Gender.Text; set => L_Gender.Text = value; }
        public string Nature { get => L_Nature.Text; set => L_Nature.Text = value; }
        public string Ability { get => L_Ability.Text; set => L_Ability.Text = value; }
        public string Item { get => L_Item.Text; set => L_Item.Text = value; }

        private string _tsv;
        public string TSV
        {
            get => _tsv;
            set
            {
                _tsv = value;
                SetPSVTSVLabel();
            }
        }

        private string _psv;
        public string PSV
        {
            get => _psv; set
            {
                _psv = value;
                SetPSVTSVLabel();
            }
        }

        public string HiddenPower { get => L_HiddenPower.Text; set => L_HiddenPower.Text = value; }
        public string Friendship { get => L_Friendship.Text; set => L_Friendship.Text = value; }

        public string HP_Stat { get => L_HP_Stat.Text; set => L_HP_Stat.Text = value; }
        public string HP_IV { get => L_HP_IV.Text; set => SetIVLabel(L_HP_IV, value); }
        public string HP_EV { get => L_HP_EV.Text; set => L_HP_EV.Text = value; }

        public string Atk_Stat { get => L_Atk_Stat.Text; set => L_Atk_Stat.Text = value; }
        public string Atk_IV { get => L_Atk_IV.Text; set => SetIVLabel(L_Atk_IV, value); }
        public string Atk_EV { get => L_Atk_EV.Text; set => L_Atk_EV.Text = value; }

        public string Def_Stat { get => L_Def_Stat.Text; set => L_Def_Stat.Text = value; }
        public string Def_IV { get => L_Def_IV.Text; set => SetIVLabel(L_Def_IV, value); }
        public string Def_EV { get => L_Def_EV.Text; set => L_Def_EV.Text = value; }

        public string SpA_Stat { get => L_SpA_Stat.Text; set => L_SpA_Stat.Text = value; }
        public string SpA_IV { get => L_SpA_IV.Text; set => SetIVLabel(L_SpA_IV, value); }
        public string SpA_EV { get => L_SpA_EV.Text; set => L_SpA_EV.Text = value; }

        public string SpD_Stat { get => L_SpD_Stat.Text; set => L_SpD_Stat.Text = value; }
        public string SpD_IV { get => L_SpD_IV.Text; set => SetIVLabel(L_SpD_IV, value); }
        public string SpD_EV { get => L_SpD_EV.Text; set => L_SpD_EV.Text = value; }

        public string Speed_Stat { get => L_Speed_Stat.Text; set => L_Speed_Stat.Text = value; }
        public string Speed_IV { get => L_Speed_IV.Text; set => SetIVLabel(L_Speed_IV, value); }
        public string Speed_EV { get => L_Speed_EV.Text; set => L_Speed_EV.Text = value; }

        public string Move1 { get => L_Move1.Text; set => L_Move1.Text = value; }
        public string MovePP1 { get => L_MovePP1.Text; set => L_MovePP1.Text = value; }

        public string Move2 { get => L_Move2.Text; set => L_Move2.Text = value; }
        public string MovePP2 { get => L_MovePP2.Text; set => L_MovePP2.Text = value; }

        public string Move3 { get => L_Move3.Text; set => L_Move3.Text = value; }
        public string MovePP3 { get => L_MovePP3.Text; set => L_MovePP3.Text = value; }

        public string Move4 { get => L_Move4.Text; set => L_Move4.Text = value; }
        public string MovePP4 { get => L_MovePP4.Text; set => L_MovePP4.Text = value; }

        private void SetPSVTSVLabel()
        {
            L_PSV_TSV.Text = $"{_psv}/{_tsv}";
            L_PSV_TSV.ForeColor = _psv == _tsv ? Color.Green : Color.Black;
            L_PSV_TSV.Font = _psv == _tsv ? new Font(DefaultFont, FontStyle.Bold) : new Font(DefaultFont, FontStyle.Regular);
        }

        private static void SetIVLabel(Control label, string text)
        {
            label.Text = text;

            var value = int.Parse(text);

            if (value == 0 || value == 1)
            {
                label.ForeColor = Color.Red;
                label.Font = new Font(DefaultFont, FontStyle.Bold);
            }
            else if (value == 30 || value == 31)
            {
                label.ForeColor = Color.Green;
                label.Font = new Font(DefaultFont, FontStyle.Bold);
            }
            else
            {
                label.ForeColor = Color.Black;
                label.Font = new Font(DefaultFont, FontStyle.Regular);
            }
        }
    }
}
