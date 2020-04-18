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

        private static readonly ItemType[] BLACKLISTED_TYPES = new ItemType[]
        {
            ItemType.DIALOGUE_LINE,
            ItemType.DIALOGUE,
            // the following have no GameDataItems in the base files
            ItemType.LOCATION,
            ItemType.WAR_SAVESTATE,
            ItemType.NULL_ITEM,
            ItemType.ZONE_MAP,
            ItemType.WORLDMAP_CHARACTER,
            ItemType.CHARACTER_APPEARANCE_OLD,
            ItemType.TECHTREE,
            ItemType.AI_STATE,
            ItemType.INSTANCE_COLLECTION,
            ItemType.TEMPORARY_INFO,
            ItemType.MOD_FILENAME,
            ItemType.PLATOON,
            ItemType.GAMESTATE_BUILDING,
            ItemType.GAMESTATE_CHARACTER,
            ItemType.GAMESTATE_FACTION,
            ItemType.GAMESTATE_TOWN_INSTANCE_LIST,
            ItemType.STATE,
            ItemType.SAVED_STATE,
            ItemType.INVENTORY_STATE,
            ItemType.INVENTORY_ITEM_STATE,
            ItemType.GAMESTATE_BUILDING_INTERIOR,
            ItemType.LOCATION_NODE,
            ItemType.MEDICAL_STATE,
            ItemType.MEDICAL_PART_STATE,
            ItemType.GAMESTATE_CRAFTING,
            ItemType.CHARACTER_APPEARANCE,
            ItemType.GAMESTATE_AI,
            ItemType.HUMAN_CHARACTER,
            ItemType.AI_SCHEDULE,
            ItemType.NEST,
            ItemType.BLUEPRINT,
            ItemType.SHOP_TRADER_CLASS,
            ItemType.GAMESTATE_TOWN,
            ItemType.TUTORIAL,
            ItemType.TERRAIN_DECALS,
            ItemType.BOAT,
            ItemType.GAMESTATE_BOAT,
            ItemType.BUILD_GRID,
            ItemType.OBJECT_TYPE_MAX
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string[] filesToLoad = { "gamedata.base", "Newwworld.mod", "Dialogue.mod", "rebirth.mod" };
            // TODO: SELECT KENSHI DIRECTORY dialog
            // TODO: Cleanup

            // TODO: Handle mod directory
            GameData data = null;
            foreach (var fileName in filesToLoad)
            {
                using var dataReader = new GameDataReader(Path.Combine(DataDirectory, fileName));
                data = dataReader.Load(data);
            }

            var itemType = typeof(ItemType);

            foreach (ItemType type in Enum.GetValues(itemType))
            {
                if (BLACKLISTED_TYPES.Contains(type))
                {
                    continue;
                }

                var item = new TreeViewItem()
                {
                    Header = type.ToString()
                };

                var gameDataItems = data.GetItemsOfType(type);

                foreach (var gameDataItem in gameDataItems)
                { 
                    var subItem = new TreeViewItem()
                    {
                        Header = $"{gameDataItem.Name} ({gameDataItem.RefCount})"
                    };

                    foreach (var referenceTypePair in gameDataItem.References)
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
    }
}
