﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LaptopAlarm.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Control,Alt,A")]
        public string ArmShortcut {
            get {
                return ((string)(this["ArmShortcut"]));
            }
            set {
                this["ArmShortcut"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Control,Alt,D")]
        public string DisarmShortcut {
            get {
                return ((string)(this["DisarmShortcut"]));
            }
            set {
                this["DisarmShortcut"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string CustomAudioFilePath {
            get {
                return ((string)(this["CustomAudioFilePath"]));
            }
            set {
                this["CustomAudioFilePath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool trigger_power {
            get {
                return ((bool)(this["trigger_power"]));
            }
            set {
                this["trigger_power"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool trigger_usb {
            get {
                return ((bool)(this["trigger_usb"]));
            }
            set {
                this["trigger_usb"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool onalarm_audio {
            get {
                return ((bool)(this["onalarm_audio"]));
            }
            set {
                this["onalarm_audio"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("defaultSound")]
        public global::LaptopAlarm.onalarm_audio_settings onalarm_audio_default {
            get {
                return ((global::LaptopAlarm.onalarm_audio_settings)(this["onalarm_audio_default"]));
            }
            set {
                this["onalarm_audio_default"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string email_to {
            get {
                return ((string)(this["email_to"]));
            }
            set {
                this["email_to"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string email_smtp_server {
            get {
                return ((string)(this["email_smtp_server"]));
            }
            set {
                this["email_smtp_server"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool email_smtp_auth {
            get {
                return ((bool)(this["email_smtp_auth"]));
            }
            set {
                this["email_smtp_auth"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string email_smtp_auth_username {
            get {
                return ((string)(this["email_smtp_auth_username"]));
            }
            set {
                this["email_smtp_auth_username"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string email_smtp_auth_password {
            get {
                return ((string)(this["email_smtp_auth_password"]));
            }
            set {
                this["email_smtp_auth_password"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool onalarm_email {
            get {
                return ((bool)(this["onalarm_email"]));
            }
            set {
                this["onalarm_email"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool onalarm_audio_volincrease {
            get {
                return ((bool)(this["onalarm_audio_volincrease"]));
            }
            set {
                this["onalarm_audio_volincrease"] = value;
            }
        }
    }
}
