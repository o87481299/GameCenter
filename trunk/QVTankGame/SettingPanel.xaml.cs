﻿using System.Windows.Controls;

namespace QGameCenterLogic
{
    /// <summary>
    /// SettingPanel.xaml 的交互逻辑
    /// </summary>
    public partial class SettingPanel : UserControl
    {
        private Canvas m_Parent;
        private SettingLogic m_SettingLogic;


        public SettingPanel(Canvas parent, SettingLogic settinglogic)
        {
            InitializeComponent();
            m_Parent = parent;
            m_SettingLogic = settinglogic;
        }

        public static void ShowSettingPanel(Canvas parent, SettingLogic settinglohic)
        {
            SettingPanel panel = new SettingPanel(parent, settinglohic);
            panel.m_Parent.Children.Add(panel);

            Label[] labels = new Label[4];
            labels[0] = panel.label01;
            labels[1] = panel.label02;
            labels[2] = panel.label03;
            labels[3] = panel.label04;

           
            Button[] buttons = new Button[6];
            buttons[0] = panel.CloseGameCenterButton;
            buttons[1] = panel.CloseClientsButton;
            buttons[2] = panel.ResetGameCenterButton;
            buttons[3] = panel.ResetClientsButton;
            buttons[4] = panel.ReturnButton;
            buttons[5] = panel.ResetSteamVR;

            var checkboxs = new CheckBox[4];
            checkboxs[0] = panel.checkBox1;
            checkboxs[1] = panel.checkBox2;
            checkboxs[2] = panel.checkBox3;
            checkboxs[3] = panel.checkBox4;

            panel.m_SettingLogic.BindingUI(labels, buttons, checkboxs,panel.Icon, () => { panel.m_Parent.Children.Remove(panel); });
        }

       
    }
}
