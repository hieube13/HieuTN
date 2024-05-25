﻿using Examination.Shared.enums;
using Examination.Shared.Questions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examination.Shared.Exams
{
    public class ExamDto 
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string ShortDesc { get; set; }

        public string Content { get; set; }

        public int NumberOfQuestions { get; set; }

        //public TimeSpan? Duration { get; set; }
        public int? DurationInMinutes { get; set; }

        public List<QuestionDto> Questions { get; set; }

        public Level Level { get; set; }

        public DateTime DateCreated { get; set; }

        public string? OwnerUserId { get; set; }

        public int NumberOfQuestionCorrectForPass { get; set; }

        public bool IsTimeRestricted { get; set; }
        public string? CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }
}
