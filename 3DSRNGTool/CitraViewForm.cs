using System.Windows.Forms;

namespace Pk3DSRNGTool
{
    using System;
    using System.Drawing;
    using System.Linq;
    using Citra;
    using PKHeX.Core;

    public partial class CitraViewForm : Form
    {
        private readonly GameState _gameState;

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
                }
                else
                {
                    pkm = new PK8();
                    SetPkm(pkmControl, pkm);
                }
            }
        }

        private void SetPkm(PokemonViewControl pkmControl, PKM pkm)
        {
            pkmControl.Species = ((Species)pkm.Species).ToString();

            pkmControl.Gender = ((Gender)pkm.Gender).ToString();
            pkmControl.Nature = ((Nature)pkm.Nature).ToString();
            pkmControl.Ability = ((Ability)pkm.Ability).ToString();
            pkmControl.Item = Lookup.GetHeldItem(pkm.HeldItem);
            pkmControl.PSV = pkm.PSV.ToString();
            pkmControl.TSV = pkm.TSV.ToString();
            pkmControl.HiddenPower = $"{GetHiddenPowerType(pkm)} {GetHiddenPowerPower(pkm)}";
            pkmControl.Friendship = pkm.CurrentFriendship.ToString();

            pkmControl.HP_Stat = $"{pkm.Stat_HPCurrent}/{pkm.Stat_HPMax}";
            pkmControl.HP_IV = pkm.IV_HP.ToString();
            pkmControl.HP_EV = pkm.EV_HP.ToString();

            pkmControl.Atk_Stat = pkm.Stat_ATK.ToString();
            pkmControl.Atk_IV = pkm.IV_ATK.ToString();
            pkmControl.Atk_EV = pkm.EV_ATK.ToString();

            pkmControl.Def_Stat = pkm.Stat_DEF.ToString();
            pkmControl.Def_IV = pkm.IV_DEF.ToString();
            pkmControl.Def_EV = pkm.EV_DEF.ToString();

            pkmControl.SpA_Stat = pkm.Stat_SPA.ToString();
            pkmControl.SpA_IV = pkm.IV_SPA.ToString();
            pkmControl.SpA_EV = pkm.EV_SPA.ToString();

            pkmControl.SpD_Stat = pkm.Stat_SPD.ToString();
            pkmControl.SpD_IV = pkm.IV_SPD.ToString();
            pkmControl.SpD_EV = pkm.EV_SPD.ToString();

            pkmControl.Speed_Stat = pkm.Stat_SPE.ToString();
            pkmControl.Speed_IV = pkm.IV_SPE.ToString();
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
