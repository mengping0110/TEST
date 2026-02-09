using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TEST.Models
{
    public class MemberVM
    {
		public Guid Id { get; set; }

		[Display(Name = "ID")]
		public int Pid { get; set; }

		[Display(Name = "姓名")]
		[Required(ErrorMessage = "請填寫{0}")]
		public string NAME { get; set; }

		[Display(Name = "自我介紹")]
		[Required(ErrorMessage = "請填寫{0}")]
		public string INFO { get; set; }

		[Display(Name = "建立日期")]
		public DateTime CreateDt { get; set; }

		public List<MemberVM> MemberList { get; set; } = [];

	}
}
