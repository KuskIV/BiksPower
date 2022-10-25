﻿using EnergyComparer.Handlers;
using EnergyComparer.Models;
using EnergyComparer.Programs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.TestCases
{
    public class IdleCase : ITestCase
    {
        private readonly string _name;
        private readonly string _language;
        private readonly DtoTestCase _program;

        public IdleCase(IDataHandler dataHandler)
        {
            _name = "IdleCase";
            _language = ELanguage.CSharp.ToString();
            _program = dataHandler.GetProgram(_name).Result;
        }


        public string GetLanguage()
        {
            return _language;
        }

        public string GetName()
        {
            return _name;
        }

        public DtoTestCase GetProgram()
        {
            return _program;
        }

        public void Run()
        {
            Thread.Sleep(TimeSpan.FromSeconds(30));
        }
    }
}
