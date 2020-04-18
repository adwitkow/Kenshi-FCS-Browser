using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Kenshi_FCS_Browser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string DataDirectory = "D:\\Steam\\steamapps\\common\\Kenshi\\data\\";
        private Dictionary<string, string> lines;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            lines = new Dictionary<string, string>();
            string[] filesToLoad = { "gamedata.base", "Newwworld.mod", "Dialogue.mod", "rebirth.mod" };
            // TODO: SELECT KENSHI DIRECTORY dialog
            // TODO: Cleanup

            // TODO: Handle mod directory
            GameData data = null;
            foreach (var fileName in filesToLoad)
            {
                using (var dataReader = new GameDataReader(Path.Combine(DataDirectory, fileName)))
                {
                    data = dataReader.Load(data);
                }
            }

            var itemType = typeof(ItemType);

            foreach (var type in Enum.GetValues(itemType))
            {
                var item = new TreeViewItem()
                {
                    Header = Enum.GetName(itemType, type)
                };

                foreach (var gameDataItem in data.GetItemsOfType((ItemType)type))
                {
                    if (gameDataItem.ItemType == ItemType.DIALOGUE)
                    {
                        if (gameDataItem.Name == "armour king talk to")
                        {
                            Console.WriteLine("sztop");
                        }

                        PullDialogueLines(gameDataItem.Name, gameDataItem, ref data);
                    }

                    var subItem = new TreeViewItem()
                    {
                        Header = $"{gameDataItem.Name} ({gameDataItem.RefCount})"
                    };

                    foreach (var referenceTypePair in gameDataItem.references)
                    {
                        var key = referenceTypePair.Key;
                        var references = referenceTypePair.Value;

                        var keyItem = new TreeViewItem()
                        {
                            Header = key
                        };

                        foreach (var reference in references)
                        {
                            var referenceItem = data.GetItem(reference.itemID);
                            var referenceValues = reference.Values;
                            var referenceTreeViewItem = new TreeViewItem()
                            {
                                Header = $"{referenceItem.Name} {referenceValues}"
                            };

                            keyItem.Items.Add(referenceTreeViewItem);
                        }

                        subItem.Items.Add(keyItem);
                    }

                    item.Items.Add(subItem);
                }

                DataTreeView.Items.Add(item);
            }
        }

        private void PullDialogueLines(string rootName, GameDataItem item, ref GameData data)
        {
            var references = item.references;
            if (references.Count == 0 || !references.ContainsKey("lines"))
            {
                return;
            }

            foreach (var lineItemReference in references["lines"])
            {
                var lineItem = data.GetItem(lineItemReference.itemID);

                var values = new List<string>();
                foreach (var pair in lineItem.data)
                {
                    if (!pair.Key.StartsWith("text"))
                    {
                        continue;
                    }

                    var value = pair.Value as string;

                    if (string.IsNullOrWhiteSpace(value))
                    {
                        continue;
                    }

                    if (value.Contains("crafting"))
                    {
                        Console.WriteLine("sztop");
                    }

                    values.Add(value);
                }

                if (!lines.ContainsKey(lineItem.StringId))
                {
                    lines.Add(lineItem.StringId, $"{rootName}:{string.Join("|", values)}");
                    PullDialogueLines(rootName, lineItem, ref data);
                }
            }
        }
    }
}
