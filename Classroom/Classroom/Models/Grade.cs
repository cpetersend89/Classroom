using System;
using System.ComponentModel.DataAnnotations;

namespace Classroom.Models
{
    public class Grade
    {
        public enum LetterGrades
        {
            A,
            B,
            C,
            D,
            F
        }

        public Guid Id { get; set; }

        public LetterGrades LetterGrade
        {
            get
            {
                if (GradePercentage >= 90)
                {
                    return LetterGrades.A;
                }
                if (GradePercentage >= 80 && GradePercentage < 90)
                {
                    return LetterGrades.B;
                }
                if (GradePercentage >= 70 && GradePercentage < 80)
                {
                    return LetterGrades.C;
                }
                if (GradePercentage >= 60 && GradePercentage < 70)
                {
                    return LetterGrades.D;
                }
                return LetterGrades.F;
            }
        }

        [Display(Name = "Points Received")]
        public double PointsReceived { get; set; }

        [DisplayFormat(DataFormatString = "{0:#,##0.0#}", ApplyFormatInEditMode = true)]
        public double GradePercentage { get; set; }
        public double Weight { get; set; }
        public double Average { get; set; }

        public static double GetGradePercentage(int possible, double actual)
        {
            return ((actual/possible)*100);
        }
    }

}
