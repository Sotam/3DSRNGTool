namespace Pk3DSRNGTool
{
    using System.Drawing;
    using System.Windows.Forms;
    using PKHeX.Core;

    public partial class PokemonViewControl : UserControl
    {
        public PokemonViewControl()
        {
            InitializeComponent();
        }

        public string Title { get => GB_Party.Text; set => GB_Party.Text = value; }

        private Species _species;
        public Species Species
        {
            get => _species;
            set
            {
                _species = value;
                L_Species.Text = value.ToString();
            }
        }

        private Gender _gender;
        public Gender Gender
        {
            get => _gender;
            set
            {
                _gender = value;
                L_Gender.Text = value.ToString();
            }
        }
        public string Nature { get => L_Nature.Text; set => L_Nature.Text = value; }

        private string _ability;

        public string Ability
        {
            get => _ability;
            set
            {
                _ability = value;
                SetAbilityLabel();
            }
        }

        private int _abilityType;
        public int AbilityType
        {
            get => _abilityType;
            set
            {
                _abilityType = value;
                SetAbilityLabel();
            }
        }

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
            get => _psv;
            set
            {
                _psv = value;
                SetPSVTSVLabel();
            }
        }

        public string HiddenPower { get => L_HiddenPower.Text; set => L_HiddenPower.Text = value; }
        public string Friendship { get => L_Friendship.Text; set => L_Friendship.Text = value; }

        public string HP_Stat { get => L_HP_Stat.Text; set => L_HP_Stat.Text = value; }

        private int _hp_iv;
        public int HP_IV
        {
            get => _hp_iv;
            set
            {
                _hp_iv = value;
                SetIVLabel(L_HP_IV, value);
            }
        }

        public string HP_EV { get => L_HP_EV.Text; set => L_HP_EV.Text = value; }

        public string Atk_Stat { get => L_Atk_Stat.Text; set => L_Atk_Stat.Text = value; }

        private int _atk_iv;
        public int Atk_IV
        {
            get => _atk_iv;
            set
            {
                _atk_iv = value;
                SetIVLabel(L_Atk_IV, value);
            }
        }

        public string Atk_EV { get => L_Atk_EV.Text; set => L_Atk_EV.Text = value; }

        public string Def_Stat { get => L_Def_Stat.Text; set => L_Def_Stat.Text = value; }

        private int _def_iv;
        public int Def_IV
        {
            get => _def_iv;
            set
            {
                _def_iv = value;
                SetIVLabel(L_Def_IV, value);
            }
        }

        public string Def_EV { get => L_Def_EV.Text; set => L_Def_EV.Text = value; }

        public string SpA_Stat { get => L_SpA_Stat.Text; set => L_SpA_Stat.Text = value; }

        private int _spa_iv;
        public int SpA_IV
        {
            get => _spa_iv;
            set
            {
                _spa_iv = value;
                SetIVLabel(L_SpA_IV, value);
            }
        }

        public string SpA_EV { get => L_SpA_EV.Text; set => L_SpA_EV.Text = value; }

        public string SpD_Stat { get => L_SpD_Stat.Text; set => L_SpD_Stat.Text = value; }

        private int _spd_iv;
        public int SpD_IV
        {
            get => _spd_iv;
            set
            {
                _spd_iv = value;
                SetIVLabel(L_SpD_IV, value);
            }
        }

        public string SpD_EV { get => L_SpD_EV.Text; set => L_SpD_EV.Text = value; }

        public string Speed_Stat { get => L_Speed_Stat.Text; set => L_Speed_Stat.Text = value; }

        private int _speed_iv;
        public int Speed_IV
        {
            get => _speed_iv;
            set
            {
                _speed_iv = value;
                SetIVLabel(L_Speed_IV, value);
            }
        }

        public string Speed_EV { get => L_Speed_EV.Text; set => L_Speed_EV.Text = value; }

        public string Move1 { get => L_Move1.Text; set => L_Move1.Text = value; }
        public string MovePP1 { get => L_MovePP1.Text; set => L_MovePP1.Text = value; }

        public string Move2 { get => L_Move2.Text; set => L_Move2.Text = value; }
        public string MovePP2 { get => L_MovePP2.Text; set => L_MovePP2.Text = value; }

        public string Move3 { get => L_Move3.Text; set => L_Move3.Text = value; }
        public string MovePP3 { get => L_MovePP3.Text; set => L_MovePP3.Text = value; }

        public string Move4 { get => L_Move4.Text; set => L_Move4.Text = value; }
        public string MovePP4 { get => L_MovePP4.Text; set => L_MovePP4.Text = value; }

        private void SetAbilityLabel()
        {
            var abilityTypeText = _abilityType == 4
                ? "H"
                : _abilityType.ToString();

            L_Ability.Text = $"{_ability} ({abilityTypeText})";
        }

        private void SetPSVTSVLabel()
        {
            L_PSV_TSV.Text = $"{_psv}/{_tsv}";
            L_PSV_TSV.ForeColor = _psv == _tsv ? Color.Green : Color.Black;
            L_PSV_TSV.Font = _psv == _tsv ? new Font(DefaultFont, FontStyle.Bold) : new Font(DefaultFont, FontStyle.Regular);
        }

        private static void SetIVLabel(Control label, int value)
        {
            label.Text = value.ToString();

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
