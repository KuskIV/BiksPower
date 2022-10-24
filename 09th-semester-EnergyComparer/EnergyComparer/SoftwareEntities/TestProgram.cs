﻿using CsvHelper;
using EnergyComparer.Handlers;
using EnergyComparer.Models;
using EnergyComparer.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyComparer.Programs
{
    public class TestProgram : ITestCase
    {
        public TestProgram(IDataHandler dataHandler)
        {
            _name = "TestProgram";
            _language = ELanguage.CSharp.ToString();
            _program = dataHandler.GetProgram(_name).Result;
        }
        public DtoProgram _program { get; set; }
        public string _name { get; set; }

        private string _language { get; set; }

        public string GetLanguage()
        {
            return _language;
        }

        public string GetName()
        {
            return _name;
        }

        public DtoProgram GetProgram()
        {
            return _program;
        }

        public void Run()
        {
            for (int i = 1; i <= 2; i++)
            {
                Thread.Sleep(1000);
            }
        }


    }
}