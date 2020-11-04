namespace Pk3DSRNGTool
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using Citra;
    using PKHeX.Core;
    using static Pk3DSRNGTool.StringItem;


    public partial class CitraViewForm : Form
    {
        public MainForm parentform => Program.mainform;

        private readonly GameState _gameState;

        private Gender _parent1Gender;
        private Gender _parent2Gender;

        public CitraViewForm(GameState gameState)
        {
            _gameState = gameState;
            InitializeComponent();

            // Party 'mons
            for (var i = 0; i < 6; i++)
            {
                var pokemonViewParty = new PokemonViewControl
                {
                    Location = new Point(13 + i * (178 + 6), 15),
                    Title = $"Party {i + 1}",
                    Name = $"party{i + 1}"
                };

                Controls.Add(pokemonViewParty);
            }

            // Wild encounter
            var pokemonViewWildEncounter = new PokemonViewControl
            {
                Location = new Point(13, 308),
                Title = "Wild Encounter",
                Name = "wildEncounter"
            };

            Controls.Add(pokemonViewWildEncounter);

            // Egg parents
            for (int i = 0; i < 2; i++)
            {
                var pokemonViewParent = new PokemonViewControl
                {
                    Location = new Point(13 + (i + 4) * (178 + 6), 308),
                    Title = $"Parent {i + 1}",
                    Name = $"parent{i + 1}"
                };

                Controls.Add(pokemonViewParent);
            }
        }

        private void B_UpdatePokemons_Click(object sender, System.EventArgs e)
        {
            var pkms = _gameState.GetPokemon();
            var pkmControls = Controls.OfType<PokemonViewControl>();

            PKM pkm;
            foreach (var pkmControl in pkmControls)
            {
                var key = pkmControl.Name;

                if (pkms.TryGetValue(key, out pkm))
                {
                    SetPkm(pkmControl, pkm);

                    // Set egg stuff
                    if (string.Equals(key, "parent1"))
                        _parent1Gender = (Gender)pkm.Gender;

                    if (string.Equals(key, "parent2"))
                        _parent2Gender = (Gender)pkm.Gender;
                }
                else
                {
                    pkm = new PK8();
                    SetPkm(pkmControl, pkm);
                }
            }
        }

        private void B_UseParent1_Click(object sender, EventArgs e)
        {
            SetParent("parent1");
        }

        private void B_UseParent2_Click(object sender, EventArgs e)
        {
            SetParent("parent2");

        }

        private void SetParent(string controlName)
        {
            var parent = Controls.OfType<PokemonViewControl>().SingleOrDefault(pvc => string.Equals(pvc.Name, controlName));
            if (parent == null)
            {
                MessageBox.Show($"No Egg parent {controlName} found, make sure you've left one in DayCare!");
                return;
            }

            var gen6 = false;
            var forme = 0;
            var species = (int)parent.Species;
            var t = gen6
                ? PersonalTable.ORAS.getFormeEntry(species, forme)
                : PersonalTable.USUM.getFormeEntry(species, forme);

            var tabPageEggs = Owner
                .Controls.OfType<TabControl>().Single(tc => string.Equals(tc.Name, "RNGMethod"))
                .Controls.OfType<TabPage>().Single(tc => string.Equals(tc.Name, "TP_EggRNG"));

            var parentsInfo = tabPageEggs.Controls.OfType<GroupBox>().SingleOrDefault(gb => string.Equals(gb.Name, "Parents_Info"));

            if (parentsInfo == null)
            {
                MessageBox.Show("No parents info found, is the EggRNG tab active?");
                return;
            }

            if (parent.Species != Species.Ditto)
            {
                var abilityComboBox = tabPageEggs
                    .Controls.OfType<GroupBox>().Single(gb => string.Equals(gb.Name, "Filters"))
                    .Controls.OfType<ComboBox>().Single(gb => string.Equals(gb.Name, "Ability"));

                for (var i = 1; i < 4; i++)
                    abilityComboBox.Items[i] = abilitynumstr[i] + (species > 0 ? $" - {abilitystr[t.Abilities[i - 1]]}" : string.Empty);

                abilityComboBox.DropDownWidth = species > 0 ? 100 : 74;
            }

            var box = string.Empty;
            switch (parent.Gender)
            {
                case Gender.Female:
                    box = "F";
                    break;
                case Gender.Male:
                    box = "M";
                    break;
                case Gender.Genderless when parent.Species == Species.Ditto:
                    if (controlName == "parent1")
                    {
                        if (_parent2Gender == Gender.Female)
                            box = "M";
                        else if (_parent2Gender == Gender.Male)
                            box = "F";
                    }

                    if (controlName == "parent2")
                    {
                        if (_parent1Gender == Gender.Female)
                            box = "M";
                        else if (_parent1Gender == Gender.Male)
                            box = "F";
                    }
                    break;
                case Gender.Genderless:
                    box = "M";
                    break;
            }

            if (string.IsNullOrWhiteSpace(box))
            {
                MessageBox.Show("Couldn't determine to which box it has to go..");
                return;
            }

            var ability = parentsInfo.Controls.OfType<ComboBox>().Single(cb => string.Equals(cb.Name, $"{box}_ability"));

            switch (parent.AbilityType)
            {
                case 0:
                    ability.SelectedIndex = 0;
                    break;
                case 4:
                    ability.SelectedIndex = 2;
                    break;
                default:
                    ability.SelectedIndex = parent.AbilityType - 1;
                    break;
            }

            var items = parentsInfo.Controls.OfType<ComboBox>().Single(cb => string.Equals(cb.Name, $"{box}_Items"));

            if (string.Equals(parent.Item, "none", StringComparison.InvariantCultureIgnoreCase))
                items.SelectedIndex = 0;
            else if (string.Equals(parent.Item, "everstone", StringComparison.InvariantCultureIgnoreCase))
                items.SelectedIndex = 1;
            else if (string.Equals(parent.Item, "destiny knot", StringComparison.InvariantCultureIgnoreCase))
                items.SelectedIndex = 2;
            else
                MessageBox.Show("Currently there is only support for setting Everstone and/or Destiny Knot");

            var ivs = new[]
        {
                parent.HP_IV,
                parent.Atk_IV,
                parent.Def_IV,
                parent.SpA_IV,
                parent.SpD_IV,
                parent.Speed_IV
            };

            if (string.Equals(box, "F"))
                parentform.IV_Female = ivs;
            else
                parentform.IV_Male = ivs;

            var ditto = parentsInfo.Controls.OfType<CheckBox>().Single(cb => string.Equals(cb.Name, $"{box}_ditto"));

            ditto.Checked = parent.Species == Species.Ditto;
        }

        private int GetEggGenderRationIndex(int gender)
        {
            switch (gender)
            {
                case 0x00: return 6; // Only male
                case 0x1F: return 2; // 7/1
                case 0x3F: return 3; // 3/1
                case 0x7F: return 1; // 1/1
                case 0xBF: return 4; // 1/3
                case 0xE1: return 5; // 1/7
                case 0xFE: return 6; // Only female
                case 0xFF: // Genderless
                default:
                    return 0;
            }
        }

        private void SetPkm(PokemonViewControl pkmControl, PKM pkm)
        {
            pkmControl.Species = (Species)pkm.Species;

            pkmControl.Gender = (Gender)pkm.Gender;
            pkmControl.Nature = ((Nature)pkm.Nature).ToString();

            pkmControl.Ability = ((Ability)pkm.Ability).ToString();
            pkmControl.AbilityType = pkm.DecryptedPartyData[0x15];

            pkmControl.Item = Lookup.GetHeldItem(pkm.HeldItem);
            pkmControl.PSV = pkm.PSV.ToString();
            pkmControl.TSV = pkm.TSV.ToString();
            pkmControl.HiddenPower = $"{GetHiddenPowerType(pkm)} {GetHiddenPowerPower(pkm)}";
            pkmControl.Friendship = pkm.CurrentFriendship.ToString();

            pkmControl.HP_Stat = $"{pkm.Stat_HPCurrent}/{pkm.Stat_HPMax}";
            pkmControl.HP_IV = pkm.IV_HP;
            pkmControl.HP_EV = pkm.EV_HP.ToString();

            pkmControl.Atk_Stat = pkm.Stat_ATK.ToString();
            pkmControl.Atk_IV = pkm.IV_ATK;
            pkmControl.Atk_EV = pkm.EV_ATK.ToString();

            pkmControl.Def_Stat = pkm.Stat_DEF.ToString();
            pkmControl.Def_IV = pkm.IV_DEF;
            pkmControl.Def_EV = pkm.EV_DEF.ToString();

            pkmControl.SpA_Stat = pkm.Stat_SPA.ToString();
            pkmControl.SpA_IV = pkm.IV_SPA;
            pkmControl.SpA_EV = pkm.EV_SPA.ToString();

            pkmControl.SpD_Stat = pkm.Stat_SPD.ToString();
            pkmControl.SpD_IV = pkm.IV_SPD;
            pkmControl.SpD_EV = pkm.EV_SPD.ToString();

            pkmControl.Speed_Stat = pkm.Stat_SPE.ToString();
            pkmControl.Speed_IV = pkm.IV_SPE;
            pkmControl.Speed_EV = pkm.EV_SPE.ToString();

            pkmControl.Move1 = Lookup.GetMove(pkm.Move1);
            pkmControl.MovePP1 = pkm.Move1_PP.ToString();

            pkmControl.Move2 = Lookup.GetMove(pkm.Move2);
            pkmControl.MovePP2 = pkm.Move2_PP.ToString();

            pkmControl.Move3 = Lookup.GetMove(pkm.Move3);
            pkmControl.MovePP3 = pkm.Move3_PP.ToString();

            pkmControl.Move4 = Lookup.GetMove(pkm.Move4);
            pkmControl.MovePP4 = pkm.Move4_PP.ToString();
        }

        private double BaseHiddenPower(PKM pkm, int multiply)
        {
            return ((pkm.IV_HP & 1) + (pkm.IV_ATK & 1) * 2 + (pkm.IV_DEF & 1) * 4 + (pkm.IV_SPE & 1) * 8 + (pkm.IV_SPA & 1) * 16 + (pkm.IV_SPD & 1) * 32) * multiply / 63;
        }

        private string GetHiddenPowerType(PKM pkm)
        {
            return Lookup.GetHiddenPower((int)Math.Floor(BaseHiddenPower(pkm, 15)));
        }
        private double GetHiddenPowerPower(PKM pkm)
        {
            return Math.Floor(BaseHiddenPower(pkm, 40) + 30);
        }
    }
}
