﻿using System.Collections.Generic;

namespace GazeManager.Models
{
    public class Pagination<T> where T : class
    {
        public int TotalPages { get; set; }

        public List<T> Data { get; set; }
    }
}