﻿using System;
using System.Collections.Generic;
using System.Text;

namespace exam_6
{
    class Task
    {
        public int Id { get; set; }
        public string Headline { get; set; }
        public string Name { get; set; }
        public string CreationDate { get; set; }
        public string CompleteDate { get; set; }
        public  bool NewStatus { get; set; }

        public Task()
        {

        }

        public Task(int id, string headline, string name)
        {
            Id = id;
            Headline = headline;
            Name = name;
            CreationDate = DateTime.Now.ToString("dd.MM.yyyy");
            NewStatus = true;
        }
    }
}
