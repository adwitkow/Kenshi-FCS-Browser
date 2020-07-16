using Kenshi_FCS_Browser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Kenshi_FCS_Browser_tests
{
    [TestClass]
    public class UnitTest1
    {
        private static readonly string DataDirectory = @"D:\Steam\steamapps\common\Kenshi\data\";

        [TestMethod]
        public void TestGameDataReader()
        {
            string[] filesToLoad = { "gamedata.base", "Newwworld.mod", "Dialogue.mod", "rebirth.mod" };

            GameData data = null;
            foreach (var fileName in filesToLoad)
            {
                using var dataReader = new GameDataReader(Path.Combine(DataDirectory, fileName));
                data = dataReader.Load(data);
            }

            Assert.AreEqual(54952, data.items.Count);
        }
    }
}
