﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Jibbr.ViewModels
{
    public interface IShell : IConductor, IGuardClose
    {
    }
}