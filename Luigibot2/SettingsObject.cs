﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luigibot2
{
    public class SettingsObject
    {
        public bool EightballEnabled { get; set; }
        public bool SlapEnabled { get; set; }
        public string[] UsersAllowedToDisable { get; set; }

        public SettingsObject()
        {
            EightballEnabled = true;
            SlapEnabled = true;
            UsersAllowedToDisable = new string[] { "luigifan2010", "ghosthawk", "aeromatter", "joey" };
        }
    }
}