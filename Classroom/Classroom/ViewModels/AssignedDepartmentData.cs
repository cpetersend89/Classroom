﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classroom.ViewModels
{
    public class AssignedDepartmentData
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public bool Assigned { get; set; }
    }
}