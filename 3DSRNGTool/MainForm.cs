﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Pk3DSRNGTool.Controls;
using Pk3DSRNGTool.Core;
using Pk3DSRNGTool.RNG;
using static Pk3DSRNGTool.FormUtil;
using static Pk3DSRNGTool.StringItem;

namespace Pk3DSRNGTool
{
    public partial class MainForm : Form
    {
        #region global variables
        private int MAX_RESULTS_NUM = 100000;
        public int Ver { get => Gameversion.SelectedIndex; set => Gameversion.SelectedIndex = value; }
        public string VersionStr => L_GameVersion.Text + ": " + Gameversion.SelectedItem.ToString();
        private Pokemon[] Pokemonlist;
        private Pokemon FormPM => RNGPool.PM;
        private bool Initializing = true;
        private byte Method => (byte)RNGMethod.SelectedIndex;
        private bool IsEvent => Method == 1;
        private bool IsBank => Method == 0 && ((FormPM as PKM6)?.Bank ?? false);
        private bool IsPelago => Method == 0 && ((FormPM as PKM7)?.IsPelago ?? false);
        private bool IsHorde => Method == 2 && (FormPM as PKMW6)?.Type == EncounterType.Horde;
        private bool FullInfoHorde => IsHorde && TTT.HasSeed && TTT.Method.SelectedIndex == 2; // all info of Horde is known
        private bool Gen6 => Ver < 5;
        public bool IsORAS => Ver == 2 || Ver == 3;
        private bool IsTransporter => Ver == 4;
        private bool Gen7 => 5 <= Ver && Ver < 9;
        private bool IsUltra => Ver > 6;
        private bool gen6timeline => Gen6 && CreateTimeline.Checked && TTT.HasSeed;
        private bool gen6timeline_available => Gen6 && (Method == 0 && !AlwaysSynced.Checked || Method == 2 && !IsHorde);
        private bool gen7fidgettimeline => FidgetPanel.Visible && (Fidget.Checked || XMenu.Checked);
        private bool gen7honey => Gen7 && Method == 2 && CB_Category.SelectedIndex < 3 && !SOS.Checked;
        private bool gen7fishing => Gen7 && Method == 2 && CB_Category.SelectedIndex == 3 && !SOS.Checked;
        private bool gen7misc => Gen7 && Method == 2 && CB_Category.SelectedIndex == 4 && !SOS.Checked;
        private bool gen7crabrawler => Gen7 && Method == 2 && CB_Category.SelectedIndex == 5 && !SOS.Checked;
        private bool gen7sos => Gen7 && Method == 2 && SOS.Checked;
        private bool SuctionCups => LeadAbility.SelectedIndex == (int)Lead.SuctionCups;
        private bool LinearDelay => IsPelago || gen7honey;
        private bool ShowForme => Method == 2 && ea != null && slotspecies.Any(new[] { 201, 774 }.Contains);
        private bool MenuMethod { get => FidgetPanel.Visible; set => FidgetPanel.Visible = value; }
        private byte lastgen;
        private EncounterArea ea;
        private bool IsNight => Night.Checked;
        private int[] slotspecies => ea.getSpecies(Ver, IsNight) ?? new int[0];
        private int[] SOSSlots => SOSAllies.getAllies(ea.Locationidx, (int)SlotSpecies.SelectedValue, Ver, IsNight);
        private int[] WeatherSlots => SOSAllies.getWeatherAllies(ea.Locationidx, Weather.SelectedIndex, IsUltra, IsNight);
        private byte Modelnum => (byte)(NPC.Value + 1);
        private RNGFilters filter;
        private byte lastmethod;
        List<Frame> Frames = new List<Frame>();
        List<Frame_ID> IDFrames = new List<Frame_ID>();
        List<int> OtherTSVList = new List<int>();
        public uint[] TinySeeds => TTT.Gen6Tiny;
        #endregion

        public MainForm()
        {
            InitializeComponent();
        }

        #region Form Loading
        private void MainForm_Load(object sender, EventArgs e)
        {
            Updater.CheckUpdate();
            Type dgvtype = typeof(DataGridView);
            System.Reflection.PropertyInfo dgvPropertyInfo = dgvtype.GetProperty("DoubleBuffered", System.Reflection.BindingFlags.SetProperty
                 | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            dgvPropertyInfo.SetValue(DGV, true, null);
            dgvPropertyInfo.SetValue(DGV_ID, true, null);

            DGV.AutoGenerateColumns = false;
            DGV_ID.AutoGenerateColumns = false;
            JumpFrame.Maximum = TargetFrame.Maximum =
            Frame_min.Maximum = Frame_max.Maximum = TimeSpan.Maximum = FuncUtil.MAXFRAME;

            Seed.Value = (uint)(Properties.Settings.Default.Seed);
            var LastGameversion = Properties.Settings.Default.GameVersion;
            var LastPkm = Properties.Settings.Default.Poke;
            var LastCategory = Properties.Settings.Default.Category;
            var LastMethod = Properties.Settings.Default.Method;
            var _LastMethod = Properties.Settings.Default._Method;
            var Eggseed = Properties.Settings.Default.Key;
            Key0.Value = (uint)Eggseed;
            Key1.Value = (uint)(Eggseed >> 32);
            ShinyCharm.Checked = Properties.Settings.Default.ShinyCharm;
            TSV.Value = Properties.Settings.Default.TSV;
            TRV.Value = Properties.Settings.Default.TRV;
            Loadlist(Properties.Settings.Default.TSVList);
            Advanced.Checked = Properties.Settings.Default.Advance;
            Status = new uint[] { Properties.Settings.Default.ST0, Properties.Settings.Default.ST1, Properties.Settings.Default.ST2, Properties.Settings.Default.ST3 };

            for (int i = 0; i < 6; i++)
                EventIV[i].Enabled = false;

            Gender.Items.AddRange(genderstr);
            Ball.Items.AddRange(genderstr);
            ParentNature.Items.AddRange(genderstr);
            Event_Gender.Items.AddRange(genderstr);
            Event_Nature.Items.AddRange(naturestr);
            for (int i = 0; i <= naturestr.Length; i++)
                SyncNature.Items.Add(string.Empty);

            string l = Properties.Settings.Default.Language;
            int lang = Array.IndexOf(langlist, l);
            if (lang < 0) lang = Array.IndexOf(langlist, "en");

            lindex = lang;
            ChangeLanguage(null, null);

            Gender.SelectedIndex =
            Ball.SelectedIndex =
            Ability.SelectedIndex =
            SyncNature.SelectedIndex =
            Event_Species.SelectedIndex = Event_PIDType.SelectedIndex =
            Event_Ability.SelectedIndex = Event_Gender.SelectedIndex =
            M_ability.SelectedIndex = F_ability.SelectedIndex =
            M_Items.SelectedIndex = F_Items.SelectedIndex =
            0;
            Egg_GenderRatio.SelectedIndex = 1;

            Gameversion.SelectedIndex = LastGameversion;
            RNGMethod.SelectedIndex = _LastMethod;
            RNGMethod_Changed(null, null);
            CB_Category.SelectedIndex = LastCategory < CB_Category.Items.Count ? LastCategory : 0;
            Poke.SelectedIndex = LastPkm < Poke.Items.Count ? LastPkm : 0;
            RNGMethod.SelectedIndex = LastMethod;

            ByIVs.Checked = true;
            B_ResetFrame_Click(null, null);
            Advanced_CheckedChanged(null, null);

            if (Properties.Settings.Default.OpenGen7Tool)
                M_Gen7MainRNGTool_Click(null, null);

            Profiles.ReadProfiles(); // Read all profiles
            RefreshProfile();

            var message = "Steps:\r\n" +
                          "1 Set fixed time of 2000-01-01 00:00:00 in Citra\r\n" +
                          "2 Start game and advance to desired frame (e.g. 100) and press the \"A\"-button*\r\n" +
                          "3 Use formula or use the value shown here\r\n" +
                          "3.1 time_variable = (initial_seed - save_variable) & 0xffffffff\r\n" +
                          "4 Use found save and time variable in 3DS Time Finder\r\n" +
                          "5 Set time based on found results in 3DS Time Finder and advance to the same frame as of step 2\r\n" +
                          "6 Result: you should've hit the seed after pressing the \"A\"-button*\r\n\r\n" +
                          "NOTE:\r\n" +
                          "* The base frame is the frame that an \"A\" press will lead to the game generating the seed. For X&Y this will be the first \"A\" press and for OR&AS this will be the second \"A\" press.";

            TimeVariableToolTip.ToolTipTitle = "Tip";
            TimeVariableToolTip.SetToolTip(Tip_TimeVar, message);

            Initializing = false;
        }

        private void MainForm_Close(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.Save();
            ntrhelper?.B_Disconnect_Click(null, null);
        }

        private void RefreshProfile()
        {
            CB_Profile.Items.Clear();
            L_Profile.Visible = CB_Profile.Visible = Profiles.GameProfiles.Count > 0;
            var ProfileList = Profiles.GameProfiles.Select(p => p.Description).ToArray();
            CB_Profile.Items.AddRange(ProfileList);
        }

        private void RefreshPKM()
        {
            if (Method != 0 && Method != 2) return;
            Pokemonlist = Pokemon.getSpecFormList(Ver, CB_Category.SelectedIndex, Method);
            var List = Pokemonlist.Select(s => new ComboItem(Translate(s.ToString()), s.SpecForm)).ToList();
            Poke.DisplayMember = "Text";
            Poke.ValueMember = "Value";
            Poke.DataSource = new BindingSource(List, null);
            Poke.SelectedIndex = 0;
        }

        private void RefreshCategory()
        {
            Ver = Math.Max(Ver, 0);
            CB_Category.Items.Clear();
            var Category = Pokemon.getCategoryList(Ver, Method).Select(t => Translate(t.ToString())).ToArray();
            CB_Category.Items.AddRange(Category);
            CB_Category.SelectedIndex = 0;
            RefreshPKM();
        }

        private void RefreshLocation()
        {
            int[] locationlist = null;
            if (Gen6)
                locationlist = LocationTable6.getLocation(FormPM as PKMW6, Ver < 2);
            else if (Gen7)
                locationlist = FormPM.Conceptual ? LocationTable7.getLocation(CB_Category.SelectedIndex, Ver > 6) : (FormPM as PKMW7)?.Location;

            MetLocation.Visible = SlotSpecies.Visible = L_Location.Visible = L_Slots.Visible = locationlist != null;
            if (locationlist == null)
                return;
            Locationlist = locationlist.Select(loc => new ComboItem(getlocationstr(loc, Ver), loc)).ToList();

            MetLocation.DisplayMember = "Text";
            MetLocation.ValueMember = "Value";
            MetLocation.DataSource = new BindingSource(Locationlist, null);

            RefreshWildSpecies();
        }

        private void RefreshWildSpecies()
        {
            int tmp = SlotSpecies.SelectedIndex;
            var species = slotspecies;
            var List = (Gen7 && CB_Category.SelectedIndex < 3 ? species.Skip(1) : species).Distinct().Select(SpecForm => new ComboItem(speciestr[SpecForm & 0x7FF], SpecForm)).ToList();
            if (gen7fishing)
                for (int i = 0; i < List.Count; i++)
                    List[i].Text += String.Format(" ({0}%)", WildRNG.SlotDistribution[(ea as FishingArea7).SlotType + (Bubbling.Checked ? 1 : 0)][i]);
            if (gen7misc)
                for (int i = 0; i < List.Count; i++)
                    List[i].Text += String.Format(" ({0}%)", WildRNG.SlotDistribution[(ea as MiscEncounter7).SlotType][i]);
            List = new[] { new ComboItem("-", 0) }.Concat(List).ToList();
            SlotSpecies.DisplayMember = "Text";
            SlotSpecies.ValueMember = "Value";
            SlotSpecies.DataSource = new BindingSource(List, null);
            if (0 <= tmp && tmp < SlotSpecies.Items.Count)
                SlotSpecies.SelectedIndex = tmp;
            Weather.SelectedIndex = 0;
        }

        private void LoadSlotSpeciesInfo()
        {
            int SpecForm = (int)SlotSpecies.SelectedValue;
            List<int> Slotidx = new List<int>();
            for (int i = Array.IndexOf(slotspecies, SpecForm); i > -1; i = Array.IndexOf(slotspecies, SpecForm, i + 1))
                Slotidx.Add(i);
            int offset = IsLinux ? 0 : 1;
            if (Gen6)
            {
                if (!IsHorde)
                    for (int i = 0; i < 12; i++)
                        Slot.CheckBoxItems[i + offset].Checked = Slotidx.Contains(i);
            }
            else
            {
                if (gen7honey)
                {
                    byte[] Slottype = EncounterArea7.SlotType[slotspecies[0]];
                    for (int i = 0; i < 10; i++)
                        Slot.CheckBoxItems[i + offset].Checked = Slotidx.Contains(Slottype[i]);
                }
                else if (gen7sos) // Refresh ally
                    RefreshSOSAlly();
                else // Other short slots
                    for (int i = 0; i < slotspecies.Length; i++)
                        Slot.CheckBoxItems[i + offset].Checked = Slotidx.Contains(i);
            }

            SetPersonalInfo(SpecForm > 0 ? SpecForm : FormPM.SpecForm, skip: !(SlotSpecies.SelectedIndex == 0 && gen7honey));
        }

        private void RefreshSOSAlly()
        {
            var Chancelist = WildRNG.SlotDistribution[39].Concat(new byte[] { 1, 10 }).ToArray();
            var fulllist = SOSSlots.Concat(WeatherSlots).Where(t => t != 0).ToArray();
            var list = fulllist.Distinct().Select(t => new ComboItem(speciestr[t & 0x7FF], t)).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                int SpecForm = list[i].Value;
                int k = 0;
                for (int j = Array.IndexOf(fulllist, SpecForm); j > -1; j = Array.IndexOf(fulllist, SpecForm, j + 1))
                    k += Chancelist[j];
                list[i].Text += String.Format(" ({0}%)", k);
            }
            if (list.Count == 0)
                list.Add(new ComboItem("-", 0));
            Ally.DisplayMember = "Text";
            Ally.ValueMember = "Value";
            Ally.DataSource = new BindingSource(list, null);
            Ally.SelectedIndex = 0;
        }

        private void Ally_SelectedIndexChanged(object sender, EventArgs e)
        {
            var fulllist = SOSSlots.Concat(WeatherSlots).Where(t => t != 0).ToArray();
            int SpecForm = (int)Ally.SelectedValue;
            int offset = IsLinux ? 0 : 1;
            for (int i = 0; i < fulllist.Length; i++)
                Slot.CheckBoxItems[i + offset].Checked = fulllist[i] == SpecForm;
        }
        #endregion

        #region Translation
        private string curlanguage;
        public int lindex { get => Lang.SelectedIndex; set => Lang.SelectedIndex = value; }
        private void ChangeLanguage(object sender, EventArgs e)
        {
            M_Option.DropDown.Close();
            string lang = langlist[lindex];

            if (lang == curlanguage)
                return;

            curlanguage = lang;
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(curlanguage);
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            this.TranslateInterface(curlanguage); // Translate the UI to language.
            Properties.Settings.Default.Language = curlanguage;
            Properties.Settings.Default.Save();
            TTT?.Translate();
            gen7tool?.TranslateInterface(lang);
            ntrhelper?.TranslateInterface(lang);
            miscrngtool?.Translate();
            Text = Text + $" v{Updater.CurrentVersion}" + "Beta";

            naturestr = getStringList("Natures", curlanguage);
            hpstr = getStringList("Types", curlanguage);
            speciestr = getStringList("Species", curlanguage);
            abilitystr = getStringList("Abilities", curlanguage);
            items = getStringList("Items", curlanguage);
            genderratio = getStringList("Genderratio", curlanguage);
            smlocation = getStringList("Location_sm", curlanguage);
            gen6location = getStringList("Location_xy", curlanguage);

            // Merge SM Strings
            var metSM_00000_good = (string[])smlocation.Clone();
            for (int i = 0; i < smlocation.Length; i += 2)
            {
                var nextLoc = smlocation[i + 1];
                if (!string.IsNullOrWhiteSpace(nextLoc) && nextLoc[0] != '[')
                    metSM_00000_good[i] += $" ({nextLoc})";
                if (i > 0 && !string.IsNullOrWhiteSpace(metSM_00000_good[i]) && metSM_00000_good.Take(i - 1).Contains(metSM_00000_good[i]))
                    metSM_00000_good[i] += $" ({metSM_00000_good.Take(i - 1).Count(s => s == metSM_00000_good[i]) + 1})";
            }
            metSM_00000_good.CopyTo(smlocation, 0);

            for (int i = 0; i < 4; i++)
                Event_PIDType.Items[i] = PIDTYPE_STR[lindex, i];

            ParentNature.Items[1] = Ball.Items[1] = M_ditto.Text;
            ParentNature.Items[2] = Ball.Items[2] = F_ditto.Text;

            for (int i = 0; i < Gameversion.Items.Count; i++)
                Gameversion.Items[i] = GAMEVERSION_STR[lindex, i];

            IVInputer.Translate(IVJUDGE_STR[lindex], STATS_STR);
            Frame.Parents[1] = M_ditto.Text;
            Frame.Parents[2] = F_ditto.Text;
            dgv_wurmpleevo.HeaderText = speciestr[265];
            dgv_frame0.HeaderText = dgv_IDframe.HeaderText + "1";

            RefreshCategory();
            if (Method == 2)
                RefreshLocation();

            Nature.Items.Clear();
            Nature.BlankText = ANY_STR[lindex];
            Nature.Items.AddRange(NatureList);

            SyncNature.Items[0] = NONE_STR[lindex];
            for (int i = 0; i < naturestr.Length; i++)
                Event_Nature.Items[i] = SyncNature.Items[i + 1] = naturestr[i];

            for (int i = 0; i < items.Length; i++)
                M_Items.Items[i] = F_Items.Items[i] = items[i];

            HiddenPower.Items.Clear();
            HiddenPower.BlankText = ANY_STR[lindex];
            HiddenPower.Items.AddRange(HiddenPowerList);

            GenderRatio.ValueMember = "Value";
            GenderRatio.DisplayMember = "Text";
            GenderRatio.DataSource = new BindingSource(GenderRatioList, null);
            GenderRatio.SelectedIndex = 0;

            Egg_GenderRatio.ValueMember = "Value";
            Egg_GenderRatio.DisplayMember = "Text";
            Egg_GenderRatio.DataSource = new BindingSource(GenderRatioList, null);
            Egg_GenderRatio.SelectedIndex = 1;

            LeadAbility.ValueMember = "Value";
            LeadAbility.DisplayMember = "Text";
            LeadAbility.DataSource = new BindingSource(LeadAbilityList, null);
            LeadAbility.SelectedIndex = 0;

            Event_Species.Items.Clear();
            Event_Species.Items.AddRange(new string[] { "-" }.Concat(speciestr.Skip(1).Take(Gen6 ? 721 : IsUltra ? 807 : 802)).ToArray());
            Event_Species.SelectedIndex = 0;

            TriggerMethod.Items.Clear();
            TriggerMethod.Items.AddRange(NONE_STR.Skip(lindex).Take(1).Concat(TRIGGER_STR[lindex]).ToArray());
            TriggerMethod.SelectedIndex = 0;

            // display something upon loading
            Nature.CheckBoxItems[0].Checked = true;
            Nature.CheckBoxItems[0].Checked = false;
            HiddenPower.CheckBoxItems[0].Checked = true;
            HiddenPower.CheckBoxItems[0].Checked = false;

            AlwaysSynced.Text = SYNC_STR[lindex, 0];
        }
        #endregion

        #region Basic UI
        private void VisibleTrigger(object sender, EventArgs e)
        {
            if ((sender as Control).Visible == false)
                (sender as CheckBox).Checked = false;
        }

        private void TabSelected(object sender, EventArgs e)
        {
            (sender as NumericUpDown)?.Select(0, Text.Length);
        }

        private void Status_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ST0 = St0.Value;
            Properties.Settings.Default.ST1 = St1.Value;
            Properties.Settings.Default.ST2 = St2.Value;
            Properties.Settings.Default.ST3 = St3.Value;
        }

        private void Key_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Key = (ulong)Key0.Value | ((ulong)Key1.Value << 32);
        }

        private void TSV_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.TSV = (short)TSV.Value;
        }
        private void TRV_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.TRV = (byte)TRV.Value;
        }

        private void ShinyCharm_CheckedChanged(object sender, EventArgs e)
        {
            MM_CheckedChanged(null, null);
            Properties.Settings.Default.ShinyCharm = ShinyCharm.Checked;
        }

        private void Advanced_CheckedChanged(object sender, EventArgs e)
        {
            B_GetTiny.Visible = Advanced.Checked;
            Properties.Settings.Default.Advance = Advanced.Checked;
        }

        private void Seed_ValueChanged(object sender, EventArgs e)
        {
            if (Initializing)
                return;
            Properties.Settings.Default.Seed = Seed.Value;
            Properties.Settings.Default.Save();
            miscrngtool.UpdateInfo(updateseed: true);
        }

        private void UpdateTip(string msg)
        {
            if (Tip.Visible = msg != null)
            {
                DGVToolTip.ToolTipTitle = "Tip";
                DGVToolTip.SetToolTip(RNGInfo, msg);
                DGVToolTip.SetToolTip(Tip, msg);
            }
            else
                DGVToolTip.RemoveAll();
        }

        private void Category_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Category = (byte)CB_Category.SelectedIndex;
            if (sender == SOS)
                RefreshSOSAlly();
            else
            {
                RefreshPKM();
                SOS.Visible = Gen7 && Method == 2 && (CB_Category.SelectedIndex == 0 || CB_Category.SelectedIndex > 2);
            }
            SpecialOnly.Visible = Gen7 && Method == 2 && CB_Category.SelectedIndex > 0 || gen7sos;
            FishingPanel.Visible = Bubbling.Visible = gen7fishing;
            L_TriggerMethod.Visible = TriggerMethod.Visible = gen7misc && (ea as MiscEncounter7).DelayType1 == 1;
            L_Correction.Visible = Correction.Visible = LinearDelay;
            L_Delay2.Visible = Delay2.Visible = gen7misc;
            Raining.Visible = Gen7 && !gen7sos;
            SOSPanel.Visible =
            L_SOSRNGFrame.Visible = L_SOSRNGSeed.Visible = SOSRNGFrame.Visible = SOSRNGSeed.Visible =
            ChainLength.Visible = L_ChainLength.Visible = gen7sos;
            var pmw6 = FormPM as PKMW6;
            L_HordeInfo.Visible = IsHorde;
            ChainLength.Visible = L_ChainLength.Visible |= pmw6?.Type == EncounterType.PokeRadar;
            CB_HAUnlocked.Visible = CB_3rdSlotUnlocked.Visible = pmw6?.Type == EncounterType.FriendSafari;
            ChainLength.Visible = L_ChainLength.Visible |= pmw6?.Type == EncounterType.Fishing;
            if (IsPelago)
            {
                Correction.Minimum = 0; Correction.Maximum = 255;
                Correction.Value = 0;
            }
            UpdateTTTMethod();
            CreateTimeline.Visible = TimeSpan.Visible = Gen7 && Method < 3 || MainRNGEgg.Checked || gen6timeline_available;
        }

        private void SearchMethod_CheckedChanged(object sender, EventArgs e)
        {
            IVPanel.Visible = ByIVs.Checked;
            StatPanel.Visible = ByStats.Checked;
            ShowStats.Enabled = ShowStats.Checked = ByStats.Checked;
        }

        private void SyncNature_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SyncNature.SelectedIndex > 0)
                LeadAbility.SelectedIndex = 1;
            if (AlwaysSynced.Checked && SyncNature.SelectedIndex == 0)
                Nature.ClearSelection();
        }

        private void LeadAbility_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Sync
            if (LeadAbility.SelectedIndex > 1)
                SyncNature.SelectedIndex = 0;

            // Suction Cups
            if ((FormPM as PKMW6)?.Type == EncounterType.Fishing)
                Special_th.Value = SuctionCups ? 98 : 49;
        }

        private void IVCount_ValueChanged(object sender, EventArgs e)
        {
            if (sender == IVsCount)
                PerfectIVs.Value = IVsCount.Value;
            else
                PerfectIVs.Value = Fix3v.Checked ? 3 : 0;
        }

        private bool Isforcedshiny;
        private void SwitchLock(object sender, EventArgs e)
        {
            if (Isforcedshiny)
                ShinyMark_Clear(null, null);
            else
            {
                ShinyMark.Image = Properties.Resources.Shiny;
                Isforcedshiny = true;
                ShinyLocked.Text = SHINY_STR[lindex, 1];
            }
        }
        private void ShinyMark_Clear(object sender, EventArgs e)
        {
            ShinyMark.Image = Properties.Resources.NonShiny;
            Isforcedshiny = false;
            ShinyLocked.Text = SHINY_STR[Math.Max(lindex, 0), 0];
        }
        private void ShinyOnly_CheckedChanged(object sender, EventArgs e)
        {
            SquareShinyOnly.Visible = ShinyOnly.Checked;
        }
        private void Reset_Click(object sender, EventArgs e)
        {
            PerfectIVs.Value = Method == 0 && Fix3v.Checked ? 3 : 0;
            IVlow = new int[6];
            IVup = new[] { 31, 31, 31, 31, 31, 31 };
            Stats = new int[6];
            if (Method == 2)
                Filter_Lv.Value = 0;

            Nature.ClearSelection();
            HiddenPower.ClearSelection();
            Slot.ClearSelection();
            Ball.SelectedIndex = Gender.SelectedIndex = Ability.SelectedIndex = 0;

            IVInputer.Reset();

            BlinkFOnly.Checked = SafeFOnly.Checked = SpecialOnly.Checked =
            ShinyOnly.Checked = DisableFilters.Checked = false;
        }

        private void SetAsStarting_Click(object sender, EventArgs e)
        {
            try
            {
                var f = (int)DGV.CurrentRow.Cells["dgv_Frame"].Value;
                Frame_min.Value = f;
            }
            catch { }
        }

        private void SetAsFidget_Click(object sender, EventArgs e)
        {
            try
            {
                var f = (int)DGV.CurrentRow.Cells["dgv_Frame"].Value;
                JumpFrame.Value = f;
            }
            catch { }
        }

        private void IVs_Click(object sender, EventArgs e)
        {
            switch (ModifierKeys)
            {
                case Keys.Shift: (sender as NumericUpDown).Value = 0; break;
                case Keys.Control: (sender as NumericUpDown).Value = 30; break;
                case Keys.Alt: (sender as NumericUpDown).Value = 31; break;
            }
        }

        private void B_SaveFilter_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog()
            {
                Filter = "txt files (*.txt)|*.txt",
                RestoreDirectory = true
            };
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string backupfile = saveFileDialog1.FileName;
                if (backupfile != null)
                    System.IO.File.WriteAllLines(backupfile, FilterSettings.SettingString());
            }
        }

        private void B_LoadFilter_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog OFD = new OpenFileDialog();
                DialogResult result = OFD.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string file = OFD.FileName;
                    if (System.IO.File.Exists(file))
                    {
                        string[] list = System.IO.File.ReadAllLines(file);
                        int tmp;
                        Reset_Click(null, null);
                        foreach (string str in list)
                        {
                            string[] SplitString = str.Split(new[] { " = " }, StringSplitOptions.None);
                            if (SplitString.Length < 2)
                                continue;
                            string name = SplitString[0];
                            string value = SplitString[1];
                            switch (name)
                            {
                                case "Nature":
                                    var naturelist = value.Split(',').ToArray();
                                    for (int i = naturestr.Length - 1; i >= 0; i--)
                                        if (naturelist.Contains(naturestr[i]))
                                            Nature.CheckBoxItems[i + 1].Checked = true;
                                    break;
                                case "HiddenPower":
                                    var hplist = value.Split(',').ToArray();
                                    for (int i = hpstr.Length - 2; i > 0; i--)
                                        if (hplist.Contains(hpstr[i]))
                                            HiddenPower.CheckBoxItems[i].Checked = true;
                                    break;
                                case "ShinyOnly":
                                    ShinyOnly.Checked = value == "T" || value == "True";
                                    break;
                                case "Ability":
                                    tmp = Convert.ToInt32(value);
                                    Ability.SelectedIndex = 0 < tmp && tmp < 4 ? tmp : 0;
                                    break;
                                case "Gender":
                                    tmp = Convert.ToInt32(value);
                                    Gender.SelectedIndex = 0 < tmp && tmp < 3 ? tmp : 0;
                                    break;
                                case "IVup":
                                    IVup = value.Split(',').ToArray().Select(s => Convert.ToInt32(s)).ToArray();
                                    break;
                                case "IVlow":
                                    IVlow = value.Split(',').ToArray().Select(s => Convert.ToInt32(s)).ToArray();
                                    break;
                                case "Number of Perfect IVs":
                                    tmp = Convert.ToInt32(value);
                                    PerfectIVs.Value = 0 < tmp && tmp < 7 ? tmp : 0;
                                    break;
                            }
                        }
                    }
                }
            }
            catch
            {
                Error(FILEERRORSTR[lindex]);
            }
        }

        private void CB_Profile_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CB_Profile.SelectedIndex < 0 || Profiles.GameProfiles.Count <= CB_Profile.SelectedIndex)
                return;
            var profile = Profiles.GameProfiles[CB_Profile.SelectedIndex];
            Gameversion.SelectedIndex = profile.GameVersion;
            TSV.Value = profile.TSV;
            TRV.Value = profile.TRV;
            ShinyCharm.Checked = profile.ShinyCharm;
            if (Gen7)
                Status = profile.Seeds;
            else
            {
                Key0.Value = profile.Seeds[0];
                Key1.Value = profile.Seeds[1];
            }
        }

        private void GameVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.GameVersion = (byte)Gameversion.SelectedIndex;
            miscrngtool.UpdateInfo(updategame: !Initializing);
            L_GenderList.Visible = GenderList.Visible = IsTransporter;
            byte currentgen = (byte)(Gen6 ? 6 : IsUltra ? 8 : 7);
            if (currentgen != lastgen)
            {
                var slotnum = new bool[Gen6 ? 12 : 10].Select((b, i) => (i + 1).ToString()).ToArray();
                Slot.Items.Clear();
                Slot.BlankText = "-";
                Slot.Items.AddRange(slotnum);
                Slot.CheckBoxItems[0].Checked = true;
                Slot.CheckBoxItems[0].Checked = false;

                Event_Species.Items.Clear();
                Event_Species.Items.AddRange(new string[] { "-" }.Concat(speciestr.Skip(1).Take(Gen6 ? 721 : IsUltra ? 807 : 802)).ToArray());
                Event_Species.SelectedIndex = 0;

                lastgen = currentgen;
            }

            RNGMethod_Changed(null, null);
        }

        private void RNGMethod_Changed(object sender, EventArgs e)
        {
            Properties.Settings.Default.Method = Method;

            UpdateTip(null);

            DGV.Visible = Method < 4;
            DGV_ID.Visible = Method == 4;

            RNGMethod.TabPages[Method].Controls.Add(this.RNGInfo);
            RNGMethod.TabPages[Method].Controls.Add(this.Filters);
            RNGMethod.TabPages[Method].Controls.Add(this.GB_CitraRNG);
            MainRNGEgg.Checked &= Method == 3;
            bool mainrngegg = Method == 3 && (MainRNGEgg.Checked || Gen6);
            RB_FrameRange.Checked = true;

            // Controls in CitraRNG
            GB_EggRNG.Enabled = Method == 3 && _connected;

            // Controls in RNGInfo
            AroundTarget.Visible = timedelaypanel.Visible = Method < 3 || mainrngegg;
            L_Correction.Visible = Correction.Visible = LinearDelay; // Not time-based shift
            L_Delay2.Visible = Delay2.Visible = gen7misc;
            Correction.Minimum = 1; Correction.Maximum = 50;
            ConsiderDelay.Visible = Timedelay.Visible = label10.Visible = Method < 4; // not show in toolkit
            label10.Text = Gen7 ? "+4F" : "F";
            RB_TimelineLeap.Visible = Gen7 && IsEvent;
            RB_EggShortest.Visible =
            EggPanel.Visible = EggNumber.Visible = Method == 3 && !mainrngegg;
            CreateTimeline.Visible = TimeSpan.Visible = Gen7 && Method < 3 || MainRNGEgg.Checked || gen6timeline_available;
            B_Calc.Enabled = !(Ver == 4 && 0 < Method);

            if (0 == Method || Method == 2)
            {
                int currmethod = (Method << 3) | Ver;
                if (lastmethod != currmethod)
                {
                    var poke = Poke.SelectedIndex;
                    var category = CB_Category.SelectedIndex;
                    RefreshCategory();
                    lastmethod = (byte)currmethod;
                    Properties.Settings.Default._Method = Method;
                    CB_Category.SelectedIndex = category < CB_Category.Items.Count ? category : 0;
                    Poke.SelectedIndex = poke < Poke.Items.Count ? poke : 0;
                }
                else if (Poke.Items.Count > 0 && sender != AlwaysSynced)
                    Poke_SelectedIndexChanged(null, null);
            }
            else if (IsEvent)
                Event_Species_SelectedIndexChanged(null, null);

            AssumeSynced.Visible =
            B_OpenTool.Visible = gen6timeline_available || IsHorde;

            if (MainRNGEgg.Checked)
                UpdateTip("4 to 8 NPCs");

            Bubbling.Visible = gen7fishing;
            SpecialOnly.Visible = Method == 2 && Gen7 && CB_Category.SelectedIndex > 0 || gen7sos;
            L_Ball.Visible = Ball.Visible = Gen7 && Method == 3;
            L_Slot.Visible = Slot.Visible = Method == 2;
            ByIVs.Enabled = ByStats.Enabled = Method < 3;

            Gen6EggPanel.Visible = Gen6 && Method == 3;
            NoDex.Visible = Gen7 && Method == 1;

            GB_EggSeed.Visible =
            RNGPanel.Visible =
            CitraTimeVariable.Visible =
            CitraSaveVariable.Visible =
            GB_TinyGen6.Visible =
            Tip_TimeVar.Visible = Gen6;

            B_IVInput.Visible = Gen7 && ByIVs.Checked;
            Raining.Visible =
            L_NPC.Visible = NPC.Visible =
            Day.Visible = Night.Visible =
            TinyMT_Status.Visible = Homogeneity.Visible =
            Lv_max.Visible = Lv_min.Visible = L_Lv.Visible = label9.Visible =
            GB_RNGGEN7ID.Visible =
            Filter_G7TID.Visible =
            CitraEggSeed2.Visible =
            CitraEggSeed3.Visible = Gen7;

            MM_CheckedChanged(null, null);
            CreateTimeline_CheckedChanged(null, null);
            DoubleEverstone(null, null);
            SetLeapRange();

            switch (Method)
            {
                case 0:
                    Sta_Setting.Controls.Add(EnctrPanel); Sta_Setting.Controls.Add(Raining);
                    Sta_Setting.Controls.Add(B_OpenTool); Sta_Setting.Controls.Add(AssumeSynced);
                    return;
                case 1: NPC.Value = 4; Event_CheckedChanged(null, null); return;
                case 2:
                    Wild_Setting.Controls.Add(EnctrPanel); Wild_Setting.Controls.Add(Raining);
                    Wild_Setting.Controls.Add(B_OpenTool); Wild_Setting.Controls.Add(AssumeSynced); return;
                case 3: ByIVs.Checked = true; break;
                case 4: (Gen7 ? Filter_G7TID : Filter_TID).Checked = true; break;
            }
        }

        private void CreateTimeline_CheckedChanged(object sender, EventArgs e)
        {
            Frame_max.Visible = label7.Visible =
            ConsiderDelay.Enabled = !(L_StartingPoint.Visible = CreateTimeline.Checked);
            Fidget.Enabled = XMenu.Enabled = CreateTimeline.Checked;
            if (CreateTimeline.Checked)
                ConsiderDelay.Checked = true;
            if (Gen6)
            {
                CB_3rdSlotUnlocked.Enabled = CreateTimeline.Checked;
                AssumeSynced.Checked &= AssumeSynced.Enabled = !CreateTimeline.Checked && B_OpenTool.Visible;
            }
            NPC_ValueChanged(null, null);
        }

        private void RB_TimelineLeap_CheckedChanged(object sender, EventArgs e)
        {
            XMenu.Checked = LeapRangePanel.Visible = RB_TimelineLeap.Checked;
        }

        private void SetLeapRange()
        {
            bool shortleaptype = getLeapType() == 1;
            DelayMin.Value = shortleaptype ? 1 : 10;
            DelayMax.Value = shortleaptype ? 3 : 100;
        }

        private void B_ResetFrame_Click(object sender, EventArgs e)
        {
            Frame_min.Value = FuncUtil.getstartingframe(Gameversion.SelectedIndex, MainRNGEgg.Checked ? 0 : Method);
            TargetFrame.Value = 5000;
            Frame_max.Value = 50000;
            TimeSpan.Value = 3600;
            if (0 == Method || Method == 2)
                Poke_SelectedIndexChanged(null, null);
            JumpFrame.Value = 0;
        }

        private void NPC_ValueChanged(object sender, EventArgs e)
        {
            SafeFOnly.Visible = BlinkFOnly.Visible = false;
            if (Gen7 && !CreateTimeline.Checked && (Method < 3 || MainRNGEgg.Checked))
                (NPC.Value == 0 ? BlinkFOnly : SafeFOnly).Visible = true;
            gen7tool?.UpdatePara(npc: NPC.Value);
        }

        private void Raining_CheckedChanged(object sender, EventArgs e)
        {
            gen7tool?.UpdatePara(raining: Raining.Checked);
        }

        // Wild RNG
        private void MetLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Gen7)
            {
                ea = LocationTable7.TableNow.FirstOrDefault(t => t.Locationidx == (int)MetLocation.SelectedValue);
                if (ea is EncounterArea7 tmp)
                {
                    NPC.Value = tmp.NPC;
                    Correction.Value = tmp.Correction;
                    Raining.Enabled = true;
                    Raining.Checked = tmp.Raining && !gen7sos;
                    if (CB_Category.SelectedIndex == 1 && FormPM is PKMW7 pmw7 && !pmw7.Conceptual)
                        Special_th.Value = pmw7.Rate[MetLocation.SelectedIndex];
                    if (LocationTable7.RustlingSpots.Contains(tmp.Location))
                        UpdateTip("Correction and NPC count might be different from default setting when there are rustling spots");
                    else
                        UpdateTip(null);
                    Lv_min.Value = ea.VersionDifference && (Ver == 6 || Ver == 8) ? tmp.LevelMinMoon : tmp.LevelMin;
                    Lv_max.Value = ea.VersionDifference && (Ver == 6 || Ver == 8) ? tmp.LevelMaxMoon : tmp.LevelMax;
                    if (tmp is EncounterArea_Crabrawler eac)
                        Filter_Lv.Value = eac.ScriptedLevel;
                }
                else if (ea is FishingArea7 f)
                {
                    if (LocationTable7.FishingNPCChangeSpots.Contains(f.Location))
                        UpdateTip("NPC count might be affected");
                    else
                        UpdateTip(null);
                    Raining.Enabled = true;
                    if (sender == MetLocation)
                        NPC.Value = f.NPC;
                    Lv_min.Value = f.LevelMin;
                    Lv_max.Value = f.LevelMax + (Bubbling.Checked && IsUltra ? 5 : 0);
                    BiteDelay.Increment = IsUltra ? 11 : 19; // Sometimes first try will have long delay
                    BiteDelay.Value = f.Longdelay ? IsUltra ? 89 : 97 : 78;
                    Timedelay.Value = f.Lapras ? 0 : -2;
                }
                else if (ea is MiscEncounter7 m)
                {
                    Raining.Enabled = true;
                    if (sender == MetLocation)
                    {
                        NPC.Value = m.NPC;
                        Lv_min.Value = m.LevelMin;
                        Lv_max.Value = m.LevelMax;
                        Timedelay.Value = m.Delay1;
                        Delay2.Value = m.Delay2;
                    }
                    L_TriggerMethod.Visible = TriggerMethod.Visible = gen7misc && m.DelayType1 == 1;
                }
                ChainLength.Maximum = 255;
            }
            else if (Gen6)
            {
                ea = LocationTable6.TableNow.FirstOrDefault(t => t.Locationidx == (int)MetLocation.SelectedValue);
                if (FormPM is PKMW6 pm && pm.Type == EncounterType.Fishing)
                {
                    Special_th.Value = SuctionCups ? 98 : 49;
                    ea = (ea as FishingArea6).GetRodArea(pm.Species);
                }
            }

            RefreshWildSpecies();
        }

        private void SlotSpecies_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SlotSpecies.SelectedIndex > 0 && (Filter_Lv.Value > Lv_max.Value || Filter_Lv.Value < Lv_min.Value))
                Filter_Lv.Value = 0;
            LoadSlotSpeciesInfo();
        }

        private void Special_th_ValueChanged(object sender, EventArgs e)
        {
            L_Rate.Visible = Special_th.Visible = Special_th.Value > 0;
        }

        private void DayNight_CheckedChanged(object sender, EventArgs e)
        {
            if (ea.DayNightDifference)
                RefreshWildSpecies();
            if (gen7sos)
                RefreshSOSAlly();
        }

        private void Weather_SelectedIndexChanged(object sender, EventArgs e) => RefreshSOSAlly();

        private void SetAsTarget_Click(object sender, EventArgs e)
        {
            try
            {
                TargetFrame.Value = Convert.ToDecimal(DGV.CurrentRow.Cells["dgv_Frame"].Value);
            }
            catch (NullReferenceException)
            {
                Error(NOSELECTION_STR[lindex]);
            }
        }

        private void Fidget_CheckedChanged(object sender, EventArgs e)
        {
            if (XMenu.Checked && Fidget.Checked)
                (sender == Fidget ? XMenu : Fidget).Checked = false;
            JumpFrame.Visible = Boy.Visible = Girl.Visible = Fidget.Checked || XMenu.Checked;
        }

        private void TargetFrame_ValueChanged(object sender, EventArgs e)
        {
            gen7tool?.UpdatePara(target: TargetFrame.Value);
            TTT.TargetFrame.Value = TargetFrame.Value;
        }

        private void Sta_AbilityLocked_CheckedChanged(object sender, EventArgs e)
        {
            Sta_Ability.Visible = Sta_AbilityLocked.Checked;
        }

        private void TargetMon_ValueChanged(object sender, EventArgs e)
        {
            if (GenderList.Text.Length < TargetMon.Value)
                return;
            GenderRatio.SelectedIndex = GenderList.Text[(int)TargetMon.Value - 1] == '1' ? 1 : 0;
        }

        private void DGV_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (Advanced.Checked)
                return;
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
            {
                DGVToolTip.Hide(this);
                DGVToolTip.ToolTipTitle = null;
                return;
            }
            Rectangle cellRect = DGV.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
            if (DGV.Columns[e.ColumnIndex].Name == "dgv_mark" && !CreateTimeline.Checked)
            {
                DGVToolTip.ToolTipTitle = "Marks";
                DGVToolTip.Show("-: The safe frames can be 100% predicted.\r\n"
                    + "★: One person on the map will blink soon. Warning for following frames.\r\n"
                    + (Modelnum > 1 ? "?: The spread might be affected by the history of NPC blink status, it's unsafe."
                                    : "5: This frame will survive for 5/30 second.\r\n30: This frame will survive for 1.00 second.\r\n36: This frame will survive for 1.20 second.")
                    + (AroundTarget.Checked ? "<?>: Some hidden frames" : string.Empty)
                    , this,
                    DGV.Location.X + cellRect.X + cellRect.Size.Width,
                    DGV.Location.Y + cellRect.Y + cellRect.Size.Height,
                    8000);
                return;
            }
            if (RNGPool.igenerator is Egg6 e6 && e6.IsMainRNGEgg && (DGV.Columns[e.ColumnIndex].Name == "dgv_psv" || DGV.Columns[e.ColumnIndex].Name == "dgv_pid"))
            {
                DGVToolTip.ToolTipTitle = "Tips";
                DGVToolTip.Show("This column shows the main RNG PID/ESV of the current egg (w/o mm or sc)\r\nNot the part of spread prediction of the egg seed in the same row."
                    , this,
                    DGV.Location.X + cellRect.X + cellRect.Size.Width,
                    DGV.Location.Y + cellRect.Y + cellRect.Size.Height,
                    8000);
                return;
            }
            if (Gen7 && DGV.Columns[e.ColumnIndex].Name == "dgv_shift")
            {
                DGVToolTip.ToolTipTitle = "Frame Shift for Eontimer Calibration";
                DGVToolTip.Show("This column shows frame to time conversion, i.e. 1F = 1/60 sec."
                    , this,
                    DGV.Location.X + cellRect.X + cellRect.Size.Width,
                    DGV.Location.Y + cellRect.Y + cellRect.Size.Height,
                    3000);
                return;
            }
            if (DGV.Columns[e.ColumnIndex].Name == "dgv_adv")
            {
                DGVToolTip.ToolTipTitle = "Frame Advance";
                DGVToolTip.Show(EggPanel.Visible ? RB_EggShortest.Checked ? "To reach target frame, please precisely follow the listed procedure" : "By receiving this egg." : "By recieving this Pokemon."
                    , this,
                    DGV.Location.X + cellRect.X + cellRect.Size.Width,
                    DGV.Location.Y + cellRect.Y + cellRect.Size.Height,
                    8000);
                return;
            }
            DGVToolTip.Hide(this);
        }

        public void B_gettiny_Click(object sender, EventArgs e)
        {
            NTRHelper.ntrclient.ReadTiny("IDSeed");
            if (Ver < 2)
            {
                OpenTinyTool(null, null);
                TTT.Method.SelectedIndex = 9;
            }
        }
        #endregion

        #region DataEntry
        private void SetPersonalInfo(int Species, int Forme, bool skip = false)
        {
            SyncNature.Enabled = !(FormPM?.Nature < 25) && FormPM.Syncable;

            // Load from personal table
            var t = Gen6 ? PersonalTable.ORAS.getFormeEntry(Species, Forme) : PersonalTable.USUM.getFormeEntry(Species, Forme);
            BS = new[] { t.HP, t.ATK, t.DEF, t.SPA, t.SPD, t.SPE };
            GenderRatio.SelectedValue = Species == 0 ? 0x7F : t.Gender;
            Fix3v.Checked = t.EggGroups[0] == 0x0F && (Ver < 2 || !Pokemon.BabyMons.Contains(Species)); // Undiscovered Group
            miscrngtool.UpdateInfo(catchrate: t.CatchRate, HP: Filter_Lv.Value == 0 ? -1 : (((t.HP * 2 + 31) * (int)Filter_Lv.Value) / 100) + (int)Filter_Lv.Value + 10);

            for (int i = 1; i < 4; i++)
                Ability.Items[i] = abilitynumstr[i] + (Species > 0 ? $" - {abilitystr[t.Abilities[i - 1]]}" : string.Empty);
            Ability.DropDownWidth = Species > 0 ? 100 : 74;

            // Load from Pokemonlist
            if (FormPM == null || IsEvent || skip)
                return;
            Filter_Lv.Value = FormPM.Level;
            AlwaysSynced.Checked = FormPM.AlwaysSync;
            ShinyLocked.Checked = FormPM.ShinyLocked;
            GenderRatio.SelectedValue = (int)FormPM.GenderRatio;
            AlwaysSynced.Text = SYNC_STR[lindex, FormPM.Syncable && FormPM.Nature > 25 ? 0 : 1];
            if (!FormPM.Syncable)
                SyncNature.SelectedIndex = 0;
            if (FormPM.Nature < 25)
                SyncNature.SelectedIndex = FormPM.Nature + 1;
            Fix3v.Checked &= !FormPM.Egg;
            Timedelay.Minimum = Math.Min((int)FormPM.Delay, 0);
            Timedelay.Value = FormPM.Delay;

            if (Species > 0 && !FormPM.Gift)
                miscrngtool.UpdateInfo(HP: (((t.HP * 2 + 31) * FormPM.Level) / 100) + FormPM.Level + 10);

            if (Sta_AbilityLocked.Checked = 0 < FormPM.Ability && FormPM.Ability < 5)
                Sta_Ability.SelectedIndex = FormPM.Ability >> 1; // 1/2/4 -> 0/1/2
            if (FormPM is PKM7 pm7)
            {
                NPC.Value = pm7.NPC;
                Fix3v.Checked |= pm7.iv3;
                Raining.Checked = Raining.Enabled = pm7.Raining;
                Raining.Enabled |= pm7.Conceptual;
                ShinyMark.Visible = pm7.UltraWormhole;
                MenuMethod = pm7.Conceptual || pm7.DelayType > 0 || pm7.Unstable;
                RB_TimelineLeap.Visible = !pm7.IsPelago;
                SetLeapRange();
                if (!AlwaysSynced.Checked && FormPM.Ability == 0)
                {
                    Sta_AbilityLocked.Checked = true;
                    Sta_Ability.SelectedIndex = 0;
                }
                return;
            }
            MenuMethod = false;
            RB_TimelineLeap.Visible = Gen7 && IsEvent;
            ShinyMark.Visible = IsBank;
        }

        private void SetPersonalInfo(int SpecForm, bool skip = false) => SetPersonalInfo(SpecForm & 0x7FF, SpecForm >> 11, skip);

        private void Poke_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Poke = (byte)Poke.SelectedIndex;
            Filter_Lv.Value = 0;

            int specform = (int)(Poke.SelectedValue);
            RNGPool.PM = Pokemonlist[Poke.SelectedIndex];
            SetPersonalInfo(specform);
            GenderRatio.Enabled = FormPM.Conceptual;
            if (FormPM.Conceptual && GenderRatio.Items.Count > 0)
                GenderRatio.SelectedIndex = 1;
            L_Targetmon.Visible = TargetMon.Visible = IsBank;
            if (IsBank)
                TargetMon.Maximum = (FormPM as PKM6).NumOfPkm;
            UpdateTTTMethod();
            if (Method == 2)
            {
                RefreshLocation();
                if (FormPM is PKMW7 pmw7) // For UB
                {
                    Special_th.Value = pmw7.Rate?[MetLocation.SelectedIndex] ?? (gen7sos ? 0 : Wild7.getSpecialRate(CB_Category.SelectedIndex));
                    Special_th.Enabled = pmw7.Conceptual;
                }
                else if (FormPM is PKMW6 pmw6)
                {
                    Special_th.Enabled = true;
                    switch (pmw6.Type)
                    {
                        case EncounterType.Normal: Special_th.Value = 1; break;
                        case EncounterType.FriendSafari: Special_th.Value = 13; break;
                        case EncounterType.Fishing: Special_th.Value = 49; break;
                        default: Special_th.Value = 0; break;
                    }
                }
                return;
            }
            switch (specform)
            {
                case 133 when IsUltra && FormPM.Egg:
                    UpdateTip("4 or 20 delay, depends on save"); break;
                case 382 when Gen6:
                case 383 when Gen6: // Grondon / Kyogre
                    UpdateTip("The delay varies from 2700-4000, depends on save and console"); break;
                case 772 when IsUltra: // Type:Null
                    UpdateTip("1 NPC at Aether Paradise"); break;
                case 791 when Gen7 && !IsUltra:
                case 792 when Gen7 && !IsUltra: // SolLuna
                    UpdateTip("2 or 6 NPCs, depends on save"); break;
                case 796 when IsUltra: // Xurkitree
                    UpdateTip("1 or 2 NPCs, depends on the walking Xurkitree in the background"); break;
                case 801:  // Magearna
                    UpdateTip("6 or 7 NPCs, depends on the person walking by"); break;
                case 803:  // Poipole
                    UpdateTip("8 or 9 NPCs at the top of Megalo Tower. NPC might fluctuate due to the inproper standing point"); break;
                default:
                    UpdateTip(null); break;
            }

            Sta_AbilityLocked.Enabled = Sta_Ability.Enabled =
            AlwaysSynced.Enabled =
            ShinyLocked.Enabled = Fix3v.Enabled = FormPM.Conceptual && !IsTransporter;
        }
        #endregion

        #region UI communication
        private void getsetting(IRNG rng)
        {
            DGV.CellFormatting -= new DataGridViewCellFormattingEventHandler(DGV_CellFormatting); //Stop Freshing
            Frames.Clear();
            Frames = new List<Frame>();

            filter = FilterSettings;
            RNGPool.igenerator = getGenerator(Method);
            if (MainRNGEgg.Checked)
                RNGPool.igenerator = getmaineggrng();

            Frame.showstats = ShowStats.Checked;
            int buffersize = 150;
            if (Gen7)
            {
                RNGPool.modelnumber = Modelnum;
                RNGPool.DelayTime = (int)Timedelay.Value / 2 + 2;
                RNGPool.raining = Raining.Checked;
                RNGPool.PreHoneyCorrection = (int)Correction.Value;
                RNGPool.HoneyDelay = IsUltra ? 63 : 93;
                RNGPool.ultrawild = IsUltra && Method == 2;

                if (Method == 0 && RNGPool.DelayType == 4 && ((int)Timedelay.Value & 1) == 1)
                    RNGPool.DelayType = 6;
                if (Method == 2)
                {
                    Frame.SpecialSlotStr = gen7wildtypestr[gen7sos ? 0 : CB_Category.SelectedIndex];
                    buffersize += RNGPool.modelnumber * 500;
                }
                if (RB_TimelineLeap.Checked)
                    buffersize += RNGPool.modelnumber * 20;
                if (RNGPool.Considerdelay = ConsiderDelay.Checked)
                    buffersize += RNGPool.modelnumber * RNGPool.DelayTime;
                if (FormPM is PKM7 pm7 && pm7.DelayType > 0)
                    buffersize += 3 * RNGPool.DelayTime;
                if (IsPelago)
                    buffersize += 256;
                if (Method == 3 && !MainRNGEgg.Checked)
                    buffersize = 100;
                if (Method < 3 || MainRNGEgg.Checked)
                    Frame.standard = FuncUtil.CalcFrame(seed: Seed.Value,
                        min: (int)(AroundTarget.Checked && TargetFrame.Value - 100 < Frame_min.Value ? TargetFrame.Value - 100 : Frame_min.Value),
                        max: (int)TargetFrame.Value,
                        ModelNumber: Modelnum,
                        raining: Raining.Checked,
                        fidget: gen7fidgettimeline)[0] * 2;
            }
            if (Gen6)
            {
                RNGPool.TinySynced = AssumeSynced.Checked;
                switch (Method)
                {
                    case 1: buffersize = 80; break;
                    case 3: buffersize = 4; break;
                }
                RNGPool.DelayTime = (int)Timedelay.Value;
                if (RNGPool.Considerdelay = ConsiderDelay.Checked)
                    buffersize += RNGPool.DelayTime;
                if (IsTransporter)
                    buffersize += 2000;
                if ((FormPM as PKMW6)?.Type == EncounterType.Fishing)
                    buffersize += 400; // 132 + 240
                Frame.standard = (int)TargetFrame.Value - (int)(AroundTarget.Checked ? TargetFrame.Value - 100 : Frame_min.Value);
            }
            RNGPool.CreateBuffer(rng, buffersize);
        }

        private int getLeapType()
        {
            if (IsEvent)
                return 0;
            if (MenuMethod)
                return 1;
            return 2;
        }

        private IGenerator getGenerator(byte method)
        {
            switch (method)
            {
                case 0: return getStaSettings();
                case 1: return getEventSetting();
                case 2: return getWildSetting();
                case 3: return getEggRNG();
                default: return null;
            }
        }

        private RNGFilters FilterSettings => new RNGFilters
        {
            Nature = Nature.CheckBoxItems.Skip(1).Select(e => e.Checked).ToArray(),
            HPType = HiddenPower.CheckBoxItems.Skip(1).Select(e => e.Checked).ToArray(),
            Gender = (byte)Gender.SelectedIndex,
            Ability = (byte)Ability.SelectedIndex,
            IVlow = IVlow,
            IVup = IVup,
            BS = ByStats.Checked ? BS : null,
            Stats = ByStats.Checked ? Stats : null,
            ShinyOnly = ShinyOnly.Checked,
            SquareShinyOnly = SquareShinyOnly.Checked,
            Skip = DisableFilters.Checked,
            PerfectIVs = (byte)PerfectIVs.Value,

            Level = (byte)Filter_Lv.Value,
            Slot = new bool[IsLinux ? 1 : 0].Concat(Slot.CheckBoxItems.Select(e => Gen6 && !CreateTimeline.Checked ? false : e.Checked)).ToArray(),
            SpecialOnly = SpecialOnly.Checked,

            Ball = (byte)Ball.SelectedIndex,
            NatureInheritance = (byte)ParentNature.SelectedIndex,
        };

        private FishingSetting getFishingSetting => new FishingSetting
        {
            basedelay = (int)BiteDelay.Value,
            suctioncups = SuctionCups,
            platdelay = Bubbling.Checked ? 19 : 14,
            pkmdelay = ((int)Timedelay.Value + 4) / 2,
        };

        private IDFilters getIDFilter()
        {
            IDFilters f = new IDFilters();
            if (Filter_SID.Checked) f.IDType = 1;
            else if (Filter_FullID.Checked) f.IDType = 2;
            else if (Filter_G7TID.Checked) f.IDType = 3;
            f.Skip = ID_Disable.Checked;
            f.RE = ID_RE.Checked;
            f.IDList = ID_List.Lines;
            f.TSVList = TSV_List.Lines;
            f.RandList = RandList.Lines;
            f.ParseString();
            return f;
        }

        private StationaryRNG getStaSettings()
        {
            StationaryRNG setting = Gen6 ? new Stationary6() : (StationaryRNG)new Stationary7();
            setting.Synchro_Stat = (byte)(SyncNature.SelectedIndex - 1);
            setting.TSV = (int)TSV.Value;
            setting.TRV = (byte)TRV.Value;
            setting.Level = (byte)Filter_Lv.Value;
            setting.ShinyCharm = ShinyCharm.Checked;

            if (ShinyLocked.Checked && ShinyMark.Visible && Isforcedshiny)
                setting.IsForcedShiny = true;
            if (IsPelago)
                (setting as Stationary7).PelagoShift = (byte)Correction.Value;
            if (IsBank)
            {
                Stationary6 set6 = setting as Stationary6;
                set6.Bank = true;
                set6.Target = (int)TargetMon.Value;
                var tmp = string.Empty.PadLeft(set6.Target, FuncUtil.IsRandomGender((int)GenderRatio.SelectedValue) ? '1' : '0').ToArray();
                if (IsTransporter)
                {
                    for (int i = 0; i < tmp.Length - 1 && i < GenderList.Text.Length; i++)
                        tmp[i] = GenderList.Text[i];
                    if (FormPM.Species == 151 || FormPM.Species == 251)
                        tmp[set6.Target - 1] = '2';
                    GenderList.Text = set6.GenderList = new string(tmp) + (GenderList.Text.Length > set6.Target ? GenderList.Text.Substring(set6.Target, GenderList.Text.Length - set6.Target) : string.Empty);
                }
                else
                    set6.GenderList = "000";
            }
            // Load from template
            if (!FormPM.Conceptual)
            {
                setting.UseTemplate(RNGPool.PM);
                return setting;
            }

            // Load from UI
            int gender = (int)GenderRatio.SelectedValue;
            setting.IV3 = Fix3v.Checked;
            setting.Gender = FuncUtil.getGenderRatio(gender);
            setting.RandomGender = FuncUtil.IsRandomGender(gender);
            setting.AlwaysSync = AlwaysSynced.Checked;
            setting.IsShinyLocked = ShinyLocked.Checked;
            setting.IVs = new int[] { -1, -1, -1, -1, -1, -1 };
            setting.Ability = (byte)(Sta_AbilityLocked.Checked ? Sta_Ability.SelectedIndex + 1 : 0);
            setting.SetValue();

            return setting;
        }

        private EventRNG getEventSetting()
        {
            int[] IVs = { -1, -1, -1, -1, -1, -1 };
            for (int i = 0; i < 6; i++)
                if (EventIVLocked[i].Checked)
                    IVs[i] = (int)EventIV[i].Value;
            if (IVsCount.Value > 0 && IVs.Count(iv => iv >= 0) + IVsCount.Value > 5)
            {
                Error(SETTINGERROR_STR[lindex] + L_IVsCount.Text);
                IVs = new[] { -1, -1, -1, -1, -1, -1 };
            }
            EventRNG e = Gen6 ? (EventRNG)new Event6() : new Event7();
            if (e is Event6 e6)
                e6.IsORAS = IsORAS;
            else if (e is Event7 e7)
                e7.NoDex = NoDex.Checked;
            e.Species = (short)Event_Species.SelectedIndex;
            e.Forme = (byte)Event_Forme.SelectedIndex;
            e.Level = (byte)Filter_Lv.Value;
            e.IVs = (int[])IVs.Clone();
            e.IVsCount = (byte)IVsCount.Value;
            e.YourID = YourID.Checked;
            e.PIDType = (byte)Event_PIDType.SelectedIndex;
            e.AbilityLocked = AbilityLocked.Checked;
            e.NatureLocked = NatureLocked.Checked;
            e.GenderLocked = GenderLocked.Checked;
            e.OtherInfo = OtherInfo.Checked;
            e.EC = Event_EC.Value;
            e.Ability = (byte)Event_Ability.SelectedIndex;
            e.Nature = (byte)Event_Nature.SelectedIndex;
            e.Gender = (byte)Event_Gender.SelectedIndex;
            e.IsEgg = IsEgg.Checked;
            if (e.YourID)
            {
                e.TSV = (ushort)TSV.Value;
                e.TRV = (byte)TRV.Value;
            }
            else
            {
                e.TID = (ushort)Event_TID.Value;
                e.SID = (ushort)Event_SID.Value;
                e.TSV = (ushort)((e.TID ^ e.SID) >> 4);
                e.TRV = (byte)((e.TID ^ e.SID) & 0xF);
                e.PID = Event_PID.Value;
            }
            e.GetGenderSetting();
            return e;
        }

        private WildRNG getWildSetting()
        {
            WildRNG setting = Gen6 ? new Wild6() : (WildRNG)new Wild7();
            setting.Synchro_Stat = (byte)(SyncNature.SelectedIndex - 1);
            switch ((Lead)LeadAbility.SelectedIndex)
            {
                case Lead.Static: setting.Static = true; break;
                case Lead.MagnetPull: setting.Magnet = true; break;
                case Lead.CuteCharmF: setting.CuteCharmGender = 1; break;
                case Lead.CuteCharmM: setting.CuteCharmGender = 2; break;
                case Lead.PressureHustleSpirit: setting.ModifiedLevel = 101; break;
                case Lead.BlackFlute: setting.Flute = +1; break;
                case Lead.WhiteFlute: setting.Flute = -1; break;
            }
            setting.TSV = (int)TSV.Value;
            setting.TRV = (byte)TRV.Value;
            setting.ShinyCharm = ShinyCharm.Checked;

            int slottype = 0;
            if (setting is Wild7 setting7)
            {
                setting7.Levelmin = (byte)Lv_min.Value;
                setting7.Levelmax = (byte)Lv_max.Value;
                setting7.SpecialEnctr = (byte)(gen7sos ? 0 : Special_th.Value);
                setting7.UB = CB_Category.SelectedIndex == 1;
                setting7.CompoundEye = LeadAbility.SelectedIndex == (int)Lead.CompoundEyes;
                if (gen7honey)
                {
                    RNGPool.DelayType = 0;
                    setting7.SpecForm = new int[11];
                    if (ea.Locationidx == 1190) slottype = 1; // Poni Plains -4
                    for (int i = 1; i < 11; i++)
                        setting7.SpecForm[i] = slotspecies[EncounterArea7.SlotType[slotspecies[0]][i - 1]];
                    if (setting7.SpecialEnctr > 0)
                    {
                        setting7.SpecForm[0] = FormPM.SpecForm;
                        setting7.SpecialLevel = FormPM.Level;
                    }
                }
                else if (gen7fishing)
                {
                    setting7.Fishing = true;
                    RNGPool.DelayType = (byte)(ConsiderDelay.Checked && !CreateTimeline.Checked && Overview.Checked ? 2 : 1);
                    RNGPool.fsetting = getFishingSetting;
                    setting7.SpecForm = new[] { 0 }.Concat(slotspecies).ToArray();
                    setting7.HookedItemSlot = (ea as FishingArea7).getitemslots(Bubbling.Checked && IsUltra);
                    slottype = (ea as FishingArea7).SlotType + (Bubbling.Checked ? 1 : 0);
                }
                else if (gen7misc)
                {
                    RNGPool.DelayType = TriggerMethod.Visible && TriggerMethod.SelectedIndex > 0 ? (byte)(TriggerMethod.SelectedIndex + 2) : (ea as MiscEncounter7).DelayType1;
                    RNGPool.WildCry = (ea as MiscEncounter7).Cry;
                    setting7.DelayType = (ea as MiscEncounter7).DelayType2;
                    setting7.DelayTime = (int)Delay2.Value / 2;
                    setting7.SpecForm = new[] { 0 }.Concat(slotspecies).ToArray();
                    slottype = (ea as MiscEncounter7).SlotType;
                }
                else if (gen7crabrawler)
                {
                    RNGPool.DelayType = 1;
                    setting7.Crabrawler = true;
                    setting7.SpecForm = new[] { 0, 739 };
                    setting7.ModifiedLevel = (byte)Filter_Lv.Value;
                    slottype = 42;
                }
                else if (gen7sos)
                {
                    RNGPool.DelayType = 1;
                    setting7.SOS = true;
                    setting7.SpecForm = new int[10];
                    SOSSlots.CopyTo(setting7.SpecForm, 1);
                    if (SOSRNG.Weather = WeatherSlots.Any(s => s != 0))
                        WeatherSlots.CopyTo(setting7.SpecForm, 8);
                    SOSRNG.ChainLength = (int)ChainLength.Value;
                    SOSRNG.SetBuffer(seed: SOSRNGSeed.Value, frame: (int)SOSRNGFrame.Value);
                    slottype = 39;
                }
            }
            if (setting is Wild6 setting6)
            {
                if (FormPM is PKMW6 pmw6)
                {
                    if (pmw6.Conceptual)
                        setting6.BlankGenderRatio = (int)GenderRatio.SelectedValue;
                    setting6.Wildtype = pmw6.Type;
                    setting6.IsORAS = IsORAS;
                    switch (pmw6.Type)
                    {
                        case EncounterType.Horde:
                            setting6.SpecForm = new int[6];
                            setting6.SlotLevel = new byte[6];
                            var hordeslot = (ea as HordeArea).getSpecies(Ver, (byte)SlotSpecies.SelectedIndex);
                            for (int i = 1; i < 6; i++)
                            {
                                setting6.SpecForm[i] = hordeslot[i - 1];
                                setting6.SlotLevel[i] = (ea as HordeArea).Level;
                            }
                            break;
                        case EncounterType.PokeRadar:
                            setting6.IsShinyLocked = ChainLength.Value > 0;
                            if (ChainLength.Value == 0) // First Encounter
                                goto default;
                            setting6._ivcnt = Math.Min(3, (int)ChainLength.Value / 20);
                            setting6.SpecForm = new int[1];
                            setting6.SlotLevel = new[] { (byte)Filter_Lv.Value };
                            break;
                        case EncounterType.FriendSafari:
                            slottype = (byte)(CB_3rdSlotUnlocked.Checked ? 50 : 49);
                            setting6._ivcnt = 2;
                            setting6._PIDroll_count = 4;
                            setting6.HA = CB_HAUnlocked.Checked;
                            setting6.EncounterRate = (byte)Special_th.Value;
                            setting6.SpecForm = new int[4];
                            setting6.SlotLevel = new byte[] { 30, 30, 30, 30 };
                            break;
                        case EncounterType.Fishing:
                            var Rod_area = ea as RodArea;
                            setting6.SpecForm = new int[4];
                            setting6.SlotLevel = new byte[4];
                            setting6.PartyPKM = (byte)TTT.Parameter1.Value;
                            setting6.EncounterRate = (byte)Special_th.Value;
                            slottype = 3;
                            setting6._PIDroll_count = Math.Min(40, 2 * (int)ChainLength.Value);
                            for (int i = 1; i < 4; i++)
                            {
                                setting6.SpecForm[i] = slotspecies[i - 1];
                                setting6.SlotLevel[i] = Rod_area.Level[i - 1];
                            }
                            break;
                        case EncounterType.RockSmash:
                            var RS_area = ea as RockSmashArea6;
                            setting6.SpecForm = new int[6];
                            setting6.SlotLevel = new byte[6];
                            slottype = 4;
                            for (int i = 1; i < 6; i++)
                            {
                                setting6.SpecForm[i] = slotspecies[i - 1];
                                setting6.SlotLevel[i] = RS_area.Level[i - 1];
                            }
                            break;
                        case EncounterType.Normal:
                            if (gen6timeline) setting6.EncounterRate = (byte)Special_th.Value;
                            goto default;
                        default:
                            var area = ea as EncounterArea6;
                            setting6.SpecForm = new int[13];
                            setting6.SlotLevel = new byte[13];
                            slottype = 2;
                            if (area == null || slotspecies.Length == 0)
                                break;
                            for (int i = 1; i < 13; i++)
                            {
                                setting6.SpecForm[i] = slotspecies[i - 1];
                                setting6.SlotLevel[i] = area.Level[i - 1];
                            }
                            break;
                    };
                }
            }

            setting.Markslots();
            setting.SlotSplitter = WildRNG.SlotDistribution[slottype];

            return setting;
        }

        private EggRNG getEggRNG()
        {
            var setting = Gen6 ? new Egg6() : (EggRNG)new Egg7();
            setting.FemaleIVs = IV_Female;
            setting.MaleIVs = IV_Male;
            setting.MaleItem = (byte)M_Items.SelectedIndex;
            setting.FemaleItem = (byte)F_Items.SelectedIndex;
            setting.ShinyCharm = ShinyCharm.Checked;
            setting.TSV = (ushort)TSV.Value;
            setting.TRV = (byte)TRV.Value;
            setting.Gender = FuncUtil.getGenderRatio((int)Egg_GenderRatio.SelectedValue);
            if (setting is Egg7 setting7)
            {
                setting7.Homogeneous = Homogeneity.Checked;
                setting7.FemaleIsDitto = F_ditto.Checked;
            }
            else if (setting is Egg6 setting6)
                setting6.IsMainRNGEgg = !ShinyCharm.Checked && !MM.Checked && RB_Accept.Checked;
            setting.InheritAbility = (byte)(F_ditto.Checked ? M_ability.SelectedIndex : F_ability.SelectedIndex);
            setting.MMethod = MM.Checked;
            setting.NidoType = NidoType.Checked;

            setting.ConsiderOtherTSV = ConsiderOtherTSV.Checked && (ShinyCharm.Checked || MM.Checked || Gen6 && RB_Accept.Checked);
            setting.OtherTSVs = OtherTSVList.ToArray();

            setting.MarkItem();
            return setting;
        }

        private MainEggRNG getmaineggrng()
        {
            RNGPool.CreateBuffer(new TinyMT(Status), 50);
            ResultME7.Egg = RNGPool.GenerateEgg7() as EggResult; // First Egg From Tiny
            return new MainEggRNG()
            {
                TSV = (int)TSV.Value,
                TRV = (byte)TRV.Value,
                ConsiderOtherTSV = ConsiderOtherTSV.Checked,
                OtherTSVs = OtherTSVList.ToArray()
            };
        }
        #endregion

        #region Start Calculation
        private void AdjustDGVColumns()
        {
            if (Method == 4)
            {
                dgv_ID_rand64.Visible = dgv_clock.Visible = dgv_gen7ID.Visible = Gen7;
                dgv_ID_state.Visible = dgv_ID_rand.Visible = Gen6;
                dgv_ID_rand.Visible &= Advanced.Checked;
                DGV_ID.DataSource = IDFrames;
                DGV_ID.Refresh();
                DGV_ID.CurrentCell = null;
                if (IDFrames.Count > 0) DGV_ID.FirstDisplayedScrollingRowIndex = 0;
                return;
            }
            dgv_synced.Visible = Method < 3 && FormPM.Syncable && !IsEvent;
            dgv_gender.Visible =
            dgv_nature.Visible = !IsTransporter;
            dgv_item.Visible = dgv_Lv.Visible = dgv_slot.Visible = Method == 2 && (Gen7 || Gen6 && gen6timeline || FullInfoHorde);
            dgv_rand.Visible = Gen6 || Gen7 && Method == 3 && !MainRNGEgg.Checked;
            dgv_rand.Visible &= Advanced.Checked;
            dgv_state.Visible = Gen6 && Method < 4;
            dgv_tinystate.Visible = Gen6 && (Method == 0 || Method == 2) && gen6timeline || Gen7 && Method == 3 && !MainRNGEgg.Checked;
            dgv_tinystate.HeaderText = COLUMN_STR[lindex][Gen7 ? 1 : 2];
            SetAsAfter.Visible = Gen7 && Method == 3 && !MainRNGEgg.Checked;
            SetAsCurrent.Visible = Method == 3 && !MainRNGEgg.Checked;
            SetAsStarting.Visible = Method != 3 || MainRNGEgg.Checked;
            SetAsFidget.Visible = JumpFrame.Visible;
            DumpAcceptList.Visible = RB_EggShortest.Checked;
            dgv_wurmpleevo.Visible = Advanced.Checked && Method == 3 && Egg_GenderRatio.SelectedIndex == 1;
            dgv_ball.Visible = Gen7 && Method == 3;
            dgv_form.Visible = ShowForme;
            dgv_adv.Visible = Gen7 && Method == 3 && !MainRNGEgg.Checked || IsBank || Gen6 && IsEvent;
            dgv_shift.Visible = dgv_time.Visible = !IsBank && (Gen6 || Method < 3 || MainRNGEgg.Checked);
            dgv_delay.Visible = dgv_mark.Visible = dgv_rand64.Visible = Gen7 && Method < 3 || MainRNGEgg.Checked;
            dgv_rand64.Visible |= Gen6 && Method == 3;
            dgv_rand64.HeaderText = COLUMN_STR[lindex][Gen6 ? 1 : 0];
            dgv_eggnum.Visible = EggNumber.Checked || RB_EggShortest.Checked;
            dgv_pid.Visible = dgv_psv.Visible = dgv_prv.Visible = Method < 3 || ShinyCharm.Checked || MM.Checked || MainRNGEgg.Checked || Gen6 && RB_Accept.Checked;
            dgv_pid.Visible &= dgv_EC.Visible = Advanced.Checked;
            dgv_frame0.Visible = gen7fishing && CreateTimeline.Checked || RB_TimelineLeap.Checked;
            dgv_Frame.HeaderText = gen7fishing || dgv_frame0.Visible ? !Overview.Checked || dgv_frame0.Visible ? dgv_IDframe.HeaderText + "2" : dgv_IDframe.HeaderText + "1" : dgv_IDframe.HeaderText;
            DGV.DataSource = Frames;
            DGV.CellFormatting += new DataGridViewCellFormattingEventHandler(DGV_CellFormatting);
            DGV.CurrentCell = null;
            if (Frames.Count > 0) DGV.FirstDisplayedScrollingRowIndex = 0;
        }

        private void Search_Click(object sender, EventArgs e)
        {
            if (ivmin0.Value > ivmax0.Value)
                Error(SETTINGERROR_STR[lindex] + L_H.Text);
            else if (ivmin1.Value > ivmax1.Value)
                Error(SETTINGERROR_STR[lindex] + L_A.Text);
            else if (ivmin2.Value > ivmax2.Value)
                Error(SETTINGERROR_STR[lindex] + L_B.Text);
            else if (ivmin3.Value > ivmax3.Value)
                Error(SETTINGERROR_STR[lindex] + L_C.Text);
            else if (ivmin4.Value > ivmax4.Value)
                Error(SETTINGERROR_STR[lindex] + L_D.Text);
            else if (ivmin5.Value > ivmax5.Value)
                Error(SETTINGERROR_STR[lindex] + L_S.Text);
            else if (Frame_min.Value > Frame_max.Value && RB_FrameRange.Checked)
                Error(SETTINGERROR_STR[lindex] + RB_FrameRange.Text);
            else
            {
                if (Gen6)
                    Search6();
                else
                    Search7();
                AdjustDGVColumns();
            }
            RNGPool.Clear();
            GC.Collect();
        }

        private static Font BoldFont = new Font("Microsoft Sans Serif", 8, FontStyle.Bold);
        private void DGV_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            int index = e.RowIndex;
            if (Frames.Count <= index || Frames[index].Formatted)
                return;
            var result = Frames[index].rt;
            var row = DGV.Rows[index];

            if (result.Shiny)
                row.DefaultCellStyle.BackColor = result.SquareShiny ? Color.Aqua : Color.LightCyan;
            if (Gen6 && Method == 3)
            {
                if (!MM.Checked && !ShinyCharm.Checked)
                {
                    row.DefaultCellStyle.BackColor = Color.White;
                    row.Cells["dgv_psv"].Style.BackColor = row.Cells["dgv_pid"].Style.BackColor = result.Shiny ? Color.LightCyan : DefaultBackColor;
                }
                if (index == 0) row.DefaultCellStyle.BackColor = DefaultBackColor;
            }

            if (result is ResultW7 rtw7 && rtw7.SpecialVal != null)
                row.DefaultCellStyle.BackColor = DefaultBackColor;

            Frames[index].Formatted = true;

            bool?[] ivsflag = (result as EggResult)?.InheritMaleIV ?? (result as ResultME7)?.InheritMaleIV;
            const int ivstart = 6;
            if (ivsflag != null)
            {
                if (RB_EggShortest.Checked && Frames[index].FrameUsed == EGGACCEPT_STR[lindex, 0])
                    row.DefaultCellStyle.BackColor = Color.LightGreen;
                if (RB_EggShortest.Checked && Frames[index].FrameUsed == EGGACCEPT_STR[lindex, 1])
                    row.DefaultCellStyle.BackColor = Color.LightCoral;

                for (int k = 0; k < 6; k++)
                {
                    if (ivsflag[k] != null)
                    { row.Cells[ivstart + k].Style.ForeColor = (ivsflag[k] == true) ? Color.Blue : Color.DeepPink; continue; }
                    if (result.IVs[k] > 29)
                    { row.Cells[ivstart + k].Style.ForeColor = Color.MediumSeaGreen; row.Cells[ivstart + k].Style.Font = BoldFont; }
                }
                return;
            }
            for (int k = 0; k < 6; k++)
            {
                if (result.IVs[k] < 1)
                {
                    row.Cells[ivstart + k].Style.Font = BoldFont;
                    row.Cells[ivstart + k].Style.ForeColor = Color.OrangeRed;
                }
                else if (result.IVs[k] > 29)
                {
                    row.Cells[ivstart + k].Style.Font = BoldFont;
                    row.Cells[ivstart + k].Style.ForeColor = Color.MediumSeaGreen;
                }
            }
        }
        #endregion

        #region Misc Tools
        public uint globalseed { get => Seed.Value; set => Seed.Value = value; }

        // Tools
        private IVRange IVInputer = new IVRange();
        private TinyTimelineTool TTT = new TinyTimelineTool();
        private MiscRNGTool miscrngtool = new MiscRNGTool();
        private Gen7MainRNGTool gen7tool;
        private NTRHelper ntrhelper;

        private void B_IVInput_Click(object sender, EventArgs e) => IVInputer.ShowDialog();
        private void M_Exit_Click(object sender, EventArgs e) => Close();

        //Gen6
        private void OpenTinyTool(object sender, EventArgs e)
        {
            TTT.Translate();
            UpdateTTTMethod();
            TTT.Show();
            TTT.Focus();
        }
        private void UpdateTTTMethod()
        {
            if (!B_OpenTool.Visible)
                return;
            if (Method == 0 && Gen6 && (FormPM.Species == 382 || FormPM.Species == 383))
            {
                TTT.Method.SelectedIndex = 10;
                return;
            }
            if (Method == 0 && FormPM is PKM6 p)
            {
                TTT.Method.SelectedIndex = p.InstantSync ? 0 : 1;
                TTT.UpdateTypeComboBox(p.IsSoaring ? new[] { 4 } : new[] { 0, 1, 3 });
                TTT.Delay.Value = Timedelay.Value;
                if (p.IsSoaring) TTT.Delay.Value = 14;
                if (TTT.Cry.Checked = p.Cry != 255) TTT.CryFrame.Value = 8;
            }
            if (Method == 2 && FormPM is PKMW6 pw)
            {
                TTT.Delay.Value = Timedelay.Value;
                TTT.Parameter2.Value = Special_th.Value;
                if (CB_3rdSlotUnlocked.Visible)
                    TTT.Parameter1.Value = CB_3rdSlotUnlocked.Checked ? 3 : 2;
                switch (pw.Type)
                {
                    case EncounterType.Horde:
                        TTT.Method.SelectedIndex = 2;
                        break;
                    case EncounterType.FriendSafari:
                        TTT.Method.SelectedIndex = 3;
                        break;
                    case EncounterType.PokeRadar:
                        TTT.Method.SelectedIndex = 4;
                        break;
                    case EncounterType.Fishing:
                        TTT.Method.SelectedIndex = 5;
                        break;
                    case EncounterType.RockSmash:
                        TTT.Method.SelectedIndex = 6;
                        break;
                    case EncounterType.CaveShadow:
                        TTT.Method.SelectedIndex = 7;
                        break;
                    case EncounterType.Normal:
                        TTT.Method.SelectedIndex = 8;
                        break;
                    default:
                        TTT.Method.SelectedIndex = 0;
                        break;
                }
            }
        }

        public void TryToConnectNTR(bool Oneclick = false)
        {
            if (ntrhelper == null)
            {
                ntrhelper = new NTRHelper();
                ntrhelper.Show(); ntrhelper.Hide();
            }
            try
            {
                ntrhelper.Connect(Oneclick);
            }
            catch
            {
                if (Oneclick) Error("Unable to connect to debugger");
            }
        }

        private void M_NTRHelper_Click(object sender, EventArgs e)
        {
            if (ntrhelper == null) ntrhelper = new NTRHelper();
            ntrhelper.TranslateInterface(curlanguage);
            ntrhelper.Show();
            ntrhelper.Focus();
        }
        public void OnConnected_Changed(bool IsConnected)
        {
            B_GetTiny.Enabled = IsConnected;
        }
        public void parseNTRInfo(string name, object data)
        {
            switch (name)
            {
                case "Version":
                    var newver = (byte)data;
                    if (Ver == newver)
                        return;
                    Gameversion.SelectedIndex = newver;
                    return;
                case "TSV":
                    TSV.Value = (int)data >> 4;
                    TRV.Value = (byte)((int)data & 0xF);
                    return;
                case "Seed":
                    Seed.Value = (uint)data;
                    return;
                case "EggSeed":
                    Status = (uint[])data;
                    return;
                case "IDSeed":
                    var tiny = (uint[])data;
                    ID_Tiny0.Value = tiny[0];
                    ID_Tiny1.Value = tiny[1];
                    ID_Tiny2.Value = tiny[2];
                    ID_Tiny3.Value = tiny[3];
                    return;
                case "TTT":
                    TTT.Gen6Tiny = (uint[])data;
                    if (TTT.Method.SelectedIndex == 9)
                        goto case "IDSeed";
                    return;
                case "BreakPoint":
                    int CurrentFrame = NTRHelper.ntrclient.getCurrentFrame();
                    if (CurrentFrame == NtrClient.FrameMax)
                    {
                        TTT.Calibrate(-1, 0, 0);
                        Error("Fail to calibrate! Please check your initial seed! Error code: 0x" + ((uint)data).ToString("X8"));
                        return;
                    }
                    Frame_min.Value = CurrentFrame;
                    CreateTimeline.Checked = CreateTimeline.Visible;
                    if (TTT.B_Cali.Visible)
                        return;
                    switch ((uint)data)
                    {
                        // Blink
                        case 0x72B9D0:
                        case 0x6F1324: // rand(3)
                            TTT.Calibrate(0, CurrentFrame, CurrentFrame);
                            break;
                        case 0x72B9E4:
                        case 0x6F1338: // rand(60)
                            var delay1 = TinyStatus.getcooldown1(NTRHelper.ntrclient.ReadTinyRNG().Nextuint());
                            TTT.Calibrate(1, CurrentFrame, CurrentFrame + delay1);
                            break;
                        case 0x72B9FC:
                        case 0x6F1350: // rand(3)
                            var delay2 = TinyStatus.getcooldown2(NTRHelper.ntrclient.ReadTinyRNG().Nextuint());
                            TTT.Calibrate(2, CurrentFrame, CurrentFrame + delay2);
                            break;

                        // Fidget
                        case 0x70B108:
                        case 0x736F64:
                            var delay3 = TinyStatus.getcooldown3(NTRHelper.ntrclient.ReadTinyRNG().Nextuint());
                            TTT.Calibrate(3, CurrentFrame, CurrentFrame + delay3);
                            break;

                        // Running NPC
                        case 0x7D3B28:
                        case 0x7D3F28:
                        case 0x78A983:
                            TTT.Calibrate(6, CurrentFrame, CurrentFrame + 16);
                            break;

                        // Soaring
                        case 0x72A7C8 when IsORAS:
                            var delay4 = TinyStatus.getcooldown4(NTRHelper.ntrclient.ReadTinyRNG().Nextuint());
                            TTT.Calibrate(4, CurrentFrame, CurrentFrame + delay4);
                            break;

                        // XY ID
                        case 0x42BDF8 when !IsORAS:
                            var delay5 = TinyStatus.getcooldown5(NTRHelper.ntrclient.ReadTinyRNG().Nextuint());
                            TTT.Calibrate(5, CurrentFrame, CurrentFrame + delay5);
                            break;

                        // Kyogre/Groundon
                        case 0x7BE43C when IsORAS:
                            var delay7 = TinyStatus.getcooldown7(NTRHelper.ntrclient.ReadTinyRNG().Nextuint());
                            TTT.Calibrate(7, CurrentFrame, CurrentFrame + delay7);
                            break;

                        default:
                            TTT.Calibrate(-1, 0, 0);
                            Error("Unknown Timeline Type");
                            return;
                    }
                    return;
            }
        }
        private void M_Gen6SeedFinder_Click(object sender, EventArgs e)
            => new Gen6MTSeedFinder().ShowDialog();

        //Gen7
        private void M_Gen7MainRNGTool_Click(object sender, EventArgs e)
        {
            if (gen7tool == null)
            {
                gen7tool = new Gen7MainRNGTool();
                gen7tool.UpdatePara(NPC.Value, TargetFrame.Value);
                gen7tool.Startup.Checked = Properties.Settings.Default.OpenGen7Tool;
            }
            gen7tool.TranslateInterface(curlanguage);
            gen7tool.Show();
            gen7tool.Focus();
        }
        public int IDCorrection { set => Clk_Correction.Value = value; }
        public decimal Framemin { set => Frame_min.Value = value; }

        private void M_Gen7EggSeedFinder_Click(object sender, EventArgs e)
            => new Gen7EggSeedFinder().ShowDialog();
        public void SetNewEggSeed(string seed)
        {
            RNGMethod.SelectedIndex = 3;
            Status = FuncUtil.SeedStr2Array(seed);
            B_Backup_Click(null, null);
        }
        private void M_ProfileManager_Click(object sender, EventArgs e)
        {
            new Subforms.ProfileManager().ShowDialog();
            RefreshProfile();
        }
        private void M_keyBVTool_Click(object sender, EventArgs e)
            => new KeyBV().ShowDialog();

        private void B_AddProfile_Click(object sender, EventArgs e)
        {
            new Subforms.ProfileView(null, true).ShowDialog();
            RefreshProfile();
        }

        private void MiscRNGTool_Click(object sender, EventArgs e)
            => miscrngtool.Show();
        #endregion
    }
}