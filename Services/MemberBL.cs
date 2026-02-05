using Microsoft.SqlServer.Server;
using TEST.Models;

namespace TEST.Services
{
	public class MemberBL
	{
		private readonly NetTestContext _context;

		public MemberBL(NetTestContext context)
		{
			_context = context;
		}

		public List<Member> GetMemberList()
		{
			return _context.Member.OrderBy(x => x.Pid).ToList();
		}

		public Member GetMember(int? pid) 
		{ 
			var member = _context.Member.FirstOrDefault(x => x.Pid == pid);
			return member;
		}

		public Member AddMember(Member member) 
		{
			member.ID = Guid.NewGuid();

			int maxPid = _context.Member.Any() ? _context.Member.Max(m => m.Pid) : 0;
			member.Pid = maxPid + 1;

			member.CreateDt = DateTime.Now;

			var newMember = _context.Member.Add(member).Entity;
			_context.SaveChanges();
			return newMember;
		}

		public Member UpdateMember(Member member) 
		{
			var menberEdit = _context.Member.FirstOrDefault(x => x.Pid == member.Pid);
			if (menberEdit != null)
			{
				menberEdit.NAME = member.NAME;
				menberEdit.INFO = member.INFO;
				_context.SaveChanges();
			}
			return menberEdit;
		}

		public Member DeleteMember(Member member) 
		{
			_context.Member.Remove(member);
			_context.SaveChanges();
			return member;
		}
	}
}
