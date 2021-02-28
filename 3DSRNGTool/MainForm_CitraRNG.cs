namespace Pk3DSRNGTool
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Windows.Forms;
    using Citra;
    using Magnetosphere;

    public partial class MainForm : Form
    {
        private CitraTranslator _citra;
        private Bot _citraWindow;
        private bool _connected;

        private uint _initialSeed;
        private IManager _gameState;

        private Thread _updateFramesThread;

        private static readonly object LockObject = new object();

        private void B_CitraConnect_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate if game is supported
                var validGames = new[] { "X", "Y", "OR", "AS", "Ultra Sun", "Ultra Moon" };
                var gameVersion = Gameversion.SelectedItem.ToString();

                if (!validGames.Any(g => string.Equals(g, gameVersion, StringComparison.InvariantCultureIgnoreCase)))
                {
                    MessageBox.Show($"Game '{gameVersion}' is not (yet) supported!");
                    return;
                }

                if (_connected)
                {
                    DisconnectCitra();
                }
                else
                {
                    ConnectCitra();
                }

                B_CitraConnect.Text = _connected ? "Disconnect" : "Connect";

                GB_MainRNG.Enabled = _connected;
                GB_EggRNG.Enabled = Method == 3 && _connected;
            }
            catch (Exception)
            {
                MessageBox.Show("Couldn't connect to Citra");
            }
        }

        private void B_CitraViewPokemons_Click(object sender, EventArgs e)
        {
            var citraViewForm = new CitraViewForm(_gameState);
            citraViewForm.Show(this);
        }

        private void B_CitraUseInitialSeed_Click(object sender, EventArgs e)
        {
            Seed.Value = CitraInitialSeed.Value;
        }

        private void B_CitraUseEggSeed_Click(object sender, EventArgs e)
        {
            if (Gen6)
            {
                Key0.Value = CitraEggSeed0.Value;
                Key1.Value = CitraEggSeed1.Value;
            }
            else
            {
                St0.Value = CitraEggSeed0.Value;
                St1.Value = CitraEggSeed1.Value;
                St2.Value = CitraEggSeed2.Value;
                St3.Value = CitraEggSeed3.Value;
            }
        }

        private void ConnectCitra()
        {
            _citraWindow = BotConfig.Citra.CreateBot();
            _citraWindow.Connect();
            _citra = (CitraTranslator)_citraWindow.Translator;

            _gameState = GetGameState(_citra);

            _updateFramesThread = new Thread(UpdateFrames);
            _updateFramesThread.Start();

            _connected = true;

            B_CitraConnect.Text = "Disconnect";
            GB_MainRNG.Enabled = true;
        }

        private void DisconnectCitra()
        {
            _updateFramesThread.Abort();

            _citraWindow.Disconnect();
            _citraWindow = null;
            _citra = null;

            _connected = false;

            B_CitraConnect.Text = "Connect";
            GB_MainRNG.Enabled = false;
        }

        private void B_CitraUseFrame_Click(object sender, EventArgs e)
        {
            if (decimal.TryParse(CitraFrame.Text, out var frame))
                Frame_min.Value = frame;
        }

        private void UpdateFrames()
        {
            try
            {
                while (true)
                {
                    lock (LockObject)
                    {
                        UpdateInterface();
                        Thread.Sleep((int)(CitraInterval.Value * 1000));
                    }

                    Thread.Sleep(1);
                }
            }
            catch (ThreadAbortException)
            {
                // Stopped thread
                MessageBox.Show("Connection with Citra has been terminated");
            }
            catch (Exception)
            {
                DisconnectCitra();
                MessageBox.Show("Disconnected, something went wrong!");
            }
        }

        private void UpdateInterface()
        {
            if (InvokeRequired)
            {
                Invoke(new ThreadStart(UpdateInterface));
            }
            else
            {
                _gameState.UpdateFrame();

                _initialSeed = _gameState.InitialSeed;

                CitraInitialSeed.Value = _initialSeed;
                CitraCurrentSeed.ValueUlong = _gameState.CurrentSeed;
                CitraFrame.Text = _gameState.FrameCount.ToString("N0");
                CitraFrameDifference.Text = _gameState.FrameDifference.ToString("N0");

                L_CitraEggReadyYesNo.Text = _gameState.EggReady() ? "Egg ready" : "No egg yet";

                var eggSeeds = _gameState.GetEggSeeds();
                CitraEggSeed0.Value = eggSeeds[0];
                CitraEggSeed1.Value = eggSeeds[1];

                if (Gen6)
                {
                    var tinyMT = _gameState.GetTinyMT;
                    CitraTiny0.Value = tinyMT[0];
                    CitraTiny1.Value = tinyMT[1];
                    CitraTiny2.Value = tinyMT[2];
                    CitraTiny3.Value = tinyMT[3];

                    CitraSaveVariable.Value = _gameState.GetSaveVariable;
                    CitraTimeVariable.Value = _gameState.GetTimeVariable;
                }
                else
                {
                    CitraEggSeed2.Value = eggSeeds[2];
                    CitraEggSeed3.Value = eggSeeds[3];
                }
            }
        }

        private IManager GetGameState(IDeviceRW citra)
        {
            var gameVersion = Gameversion.SelectedItem.ToString();

            switch (gameVersion)
            {
                case "X":
                case "Y":
                    return new ManagerXY(citra);
                case "OR":
                case "AS":
                    return new ManagerOmegaRubyAlphaSapphire(citra);
                case "Ultra Sun":
                case "Ultra Moon":
                    return new ManagerUltraSunUltraMoon(citra);
                default:
                    throw new NotImplementedException($"Game '{gameVersion}' is not (yet) supported!");
            }
        }
    }
}
