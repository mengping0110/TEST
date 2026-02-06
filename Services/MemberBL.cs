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

		public Member GetMember(Guid? id) 
		{ 
			var member = _context.Member.FirstOrDefault(x => x.ID == id);
			return member;
		}

		public Member AddMember(Member member) 
		{
			member.ID = Guid.NewGuid();
			member.CreateDt = DateTime.Now;

			var newMember = _context.Member.Add(member).Entity;
			_context.SaveChanges();
			return newMember;
		}

		public Member UpdateMember(Member member) 
		{

			_context.SaveChanges();
			
			return member;
		}



		public Member DeleteMember(Member member) 
		{
			_context.Member.Remove(member);
			_context.SaveChanges();
			return member;
		}
	}
}
