using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkyEditor.Core.UI;
using System;
using System.Collections.Generic;
using System.Text;
using SkyEditor.Core.IO;
using SkyEditor.Core.TestComponents;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;

namespace SkyEditor.Core.Tests.UI
{
    [TestClass]
    public class UIHelperTests
    {
        public const string TestCategory = "UI - UI Helper";

        #region Child Classes

        public class MenuActionA : MenuAction
        {
            public MenuActionA() : base(new string[] { "A" })
            {
            }

            public override void DoAction(IEnumerable<object> targets)
            {
                throw new NotImplementedException();
            }
        }

        public class MenuActionB : MenuAction
        {
            public MenuActionB() : base(new string[] { "B" })
            {
            }

            public override void DoAction(IEnumerable<object> targets)
            {
                throw new NotImplementedException();
            }
        }

        public class MenuActionAB : MenuAction
        {
            public MenuActionAB() : base(new string[] { "A", "B" })
            {
            }

            public override void DoAction(IEnumerable<object> targets)
            {
                throw new NotImplementedException();
            }
        }

        public class MenuActionDev : MenuAction
        {
            public MenuActionDev() : base(new string[] { "Dev" })
            {
                this.DevOnly = true;
            }

            public override void DoAction(IEnumerable<object> targets)
            {
                throw new NotImplementedException();
            }
        }

        public class ContextMenuActionA : MenuAction
        {
            public ContextMenuActionA() : base(new string[] { "A" })
            {
                this.IsContextBased = true;
            }

            public override IEnumerable<TypeInfo> GetSupportedTypes()
            {
                return new TypeInfo[] { typeof(string).GetTypeInfo() };
            }

            public override void DoAction(IEnumerable<object> targets)
            {
                throw new NotImplementedException();
            }
        }

        public class ContextMenuActionB : MenuAction
        {
            public ContextMenuActionB() : base(new string[] { "B" })
            {
                this.IsContextBased = true;
            }

            public override IEnumerable<TypeInfo> GetSupportedTypes()
            {
                return new TypeInfo[] { typeof(string).GetTypeInfo() };
            }

            public override void DoAction(IEnumerable<object> targets)
            {
                throw new NotImplementedException();
            }
        }

        public class ContextMenuActionAB : MenuAction
        {
            public ContextMenuActionAB() : base(new string[] { "A", "B" })
            {
                this.IsContextBased = true;
            }

            public override IEnumerable<TypeInfo> GetSupportedTypes()
            {
                return new TypeInfo[] { typeof(string).GetTypeInfo() };
            }

            public override void DoAction(IEnumerable<object> targets)
            {
                throw new NotImplementedException();
            }
        }

        public class ContextMenuActionDev : MenuAction
        {
            public ContextMenuActionDev() : base(new string[] { "Dev" })
            {
                this.IsContextBased = true;
                this.DevOnly = true;
            }

            public override IEnumerable<TypeInfo> GetSupportedTypes()
            {
                return new TypeInfo[] { typeof(string).GetTypeInfo() };
            }

            public override void DoAction(IEnumerable<object> targets)
            {
                throw new NotImplementedException();
            }
        }

        public class ContextMenuActionATarget2 : MenuAction
        {
            public ContextMenuActionATarget2() : base(new string[] { "A" })
            {
                this.IsContextBased = true;
            }

            public override IEnumerable<TypeInfo> GetSupportedTypes()
            {
                return new TypeInfo[] { typeof(UIHelperTests).GetTypeInfo() };
            }

            public override void DoAction(IEnumerable<object> targets)
            {
                throw new NotImplementedException();
            }
        }

        public class ContextMenuActionBTarget2 : MenuAction
        {
            public ContextMenuActionBTarget2() : base(new string[] { "B" })
            {
                this.IsContextBased = true;
            }

            public override IEnumerable<TypeInfo> GetSupportedTypes()
            {
                return new TypeInfo[] { typeof(UIHelperTests).GetTypeInfo() };
            }

            public override void DoAction(IEnumerable<object> targets)
            {
                throw new NotImplementedException();
            }
        }

        public class ContextMenuActionABTarget2 : MenuAction
        {
            public ContextMenuActionABTarget2() : base(new string[] { "A", "B" })
            {
                this.IsContextBased = true;
            }

            public override IEnumerable<TypeInfo> GetSupportedTypes()
            {
                return new TypeInfo[] { typeof(UIHelperTests).GetTypeInfo() };
            }

            public override void DoAction(IEnumerable<object> targets)
            {
                throw new NotImplementedException();
            }
        }

        public class ContextMenuActionDevTarget2 : MenuAction
        {
            public ContextMenuActionDevTarget2() : base(new string[] { "Dev" })
            {
                this.IsContextBased = true;
                this.DevOnly = true;
            }

            public override IEnumerable<TypeInfo> GetSupportedTypes()
            {
                return new TypeInfo[] { typeof(UIHelperTests).GetTypeInfo() };
            }

            public override void DoAction(IEnumerable<object> targets)
            {
                throw new NotImplementedException();
            }
        }


        public class CoreMod : CoreSkyEditorPlugin
        {
            public override string PluginName { get; }

            public override string PluginAuthor { get; }

            public override string Credits { get; }

            public override string GetExtensionDirectory()
            {
                return "/extensions";
            }

            public override IIOProvider GetIOProvider()
            {
                return new MemoryIOProvider();
            }

            public override void Load(PluginManager manager)
            {
                base.Load(manager);

                manager.RegisterType<MenuAction, MenuActionA>();
                manager.RegisterType<MenuAction, MenuActionB>();
                manager.RegisterType<MenuAction, MenuActionAB>();
                manager.RegisterType<MenuAction, MenuActionDev>();

                manager.RegisterType<MenuAction, ContextMenuActionA>();
                manager.RegisterType<MenuAction, ContextMenuActionB>();
                manager.RegisterType<MenuAction, ContextMenuActionAB>();
                manager.RegisterType<MenuAction, ContextMenuActionDev>();

                manager.RegisterType<MenuAction, ContextMenuActionATarget2>();
                manager.RegisterType<MenuAction, ContextMenuActionBTarget2>();
                manager.RegisterType<MenuAction, ContextMenuActionABTarget2>();
                manager.RegisterType<MenuAction, ContextMenuActionDevTarget2>();
            }
        }
        #endregion

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task GetMenuItemInfo_NoDev()
        {
            using (var manager = new PluginManager())
            {
                await manager.LoadCore(new CoreMod());
                using (var appViewModel = new ApplicationViewModel(manager))
                {
                    var info = await UIHelper.GetMenuItemInfo(appViewModel, false);
                    Assert.AreEqual(2, info.Count);
                    Assert.AreEqual(1, info.FirstOrDefault(x => x.ActionTypes.Contains(typeof(MenuActionA).GetTypeInfo())).Children.Count);
                    Assert.AreEqual(0, info.FirstOrDefault(x => x.ActionTypes.Contains(typeof(MenuActionB).GetTypeInfo())).Children.Count);
                }
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task GetMenuItemInfo_Dev()
        {
            using (var manager = new PluginManager())
            {
                await manager.LoadCore(new CoreMod());
                using (var appViewModel = new ApplicationViewModel(manager))
                {
                    var info = await UIHelper.GetMenuItemInfo(appViewModel, true);
                    Assert.AreEqual(3, info.Count);
                    Assert.AreEqual(1, info.FirstOrDefault(x => x.ActionTypes.Contains(typeof(MenuActionA).GetTypeInfo())).Children.Count);
                    Assert.AreEqual(0, info.FirstOrDefault(x => x.ActionTypes.Contains(typeof(MenuActionB).GetTypeInfo())).Children.Count);
                    Assert.AreEqual(0, info.FirstOrDefault(x => x.ActionTypes.Contains(typeof(MenuActionDev).GetTypeInfo())).Children.Count);
                }
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task GetContextMenuItemInfo_NoDev()
        {
            using (var manager = new PluginManager())
            {
                await manager.LoadCore(new CoreMod());
                using (var appViewModel = new ApplicationViewModel(manager))
                {
                    var info = await UIHelper.GetContextMenuItemInfo("a string", appViewModel, false);
                    Assert.AreEqual(2, info.Count, "Incorrect count for string target");
                    Assert.AreEqual(1, info.FirstOrDefault(x => x.ActionTypes.Contains(typeof(ContextMenuActionA).GetTypeInfo())).Children.Count, "Incorrect count for string target");
                    Assert.AreEqual(0, info.FirstOrDefault(x => x.ActionTypes.Contains(typeof(ContextMenuActionB).GetTypeInfo())).Children.Count, "Incorrect count for string target");
                }
            }

            using (var manager = new PluginManager())
            {
                await manager.LoadCore(new CoreMod());
                using (var appViewModel = new ApplicationViewModel(manager))
                {
                    var info = await UIHelper.GetContextMenuItemInfo(new UIHelperTests(), appViewModel, false);
                    Assert.AreEqual(2, info.Count, "Incorrect count for UIHelperTests target");
                    Assert.AreEqual(1, info.FirstOrDefault(x => x.ActionTypes.Contains(typeof(ContextMenuActionATarget2).GetTypeInfo())).Children.Count, "Incorrect count for UIHelperTests target");
                    Assert.AreEqual(0, info.FirstOrDefault(x => x.ActionTypes.Contains(typeof(ContextMenuActionBTarget2).GetTypeInfo())).Children.Count, "Incorrect count for UIHelperTests target");
                }
            }
        }

        [TestMethod]
        [TestCategory(TestCategory)]
        public async Task GetContextMenuItemInfo_Dev()
        {

            using (var manager = new PluginManager())
            {
                await manager.LoadCore(new CoreMod());
                using (var appViewModel = new ApplicationViewModel(manager))
                {
                    var info = await UIHelper.GetContextMenuItemInfo("a string", appViewModel, true);
                    Assert.AreEqual(3, info.Count, "Incorrect count for string target");
                    Assert.AreEqual(1, info.FirstOrDefault(x => x.ActionTypes.Contains(typeof(ContextMenuActionA).GetTypeInfo())).Children.Count, "Incorrect count for string target");
                    Assert.AreEqual(0, info.FirstOrDefault(x => x.ActionTypes.Contains(typeof(ContextMenuActionB).GetTypeInfo())).Children.Count, "Incorrect count for string target");
                    Assert.AreEqual(0, info.FirstOrDefault(x => x.ActionTypes.Contains(typeof(ContextMenuActionDev).GetTypeInfo())).Children.Count, "Incorrect count for string target");
                }
            }

            using (var manager = new PluginManager())
            {
                await manager.LoadCore(new CoreMod());
                using (var appViewModel = new ApplicationViewModel(manager))
                {
                    var info = await UIHelper.GetContextMenuItemInfo(new UIHelperTests(), appViewModel, true);
                    Assert.AreEqual(3, info.Count, "Incorrect count for UIHelperTests target");
                    Assert.AreEqual(1, info.FirstOrDefault(x => x.ActionTypes.Contains(typeof(ContextMenuActionATarget2).GetTypeInfo())).Children.Count, "Incorrect count for UIHelperTests target");
                    Assert.AreEqual(0, info.FirstOrDefault(x => x.ActionTypes.Contains(typeof(ContextMenuActionBTarget2).GetTypeInfo())).Children.Count, "Incorrect count for UIHelperTests target");
                    Assert.AreEqual(0, info.FirstOrDefault(x => x.ActionTypes.Contains(typeof(ContextMenuActionDevTarget2).GetTypeInfo())).Children.Count, "Incorrect count for UIHelperTests target");
                }
            }
        }
    }
}
