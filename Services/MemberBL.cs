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

		public List<MemberVM> GetMemberList()
		{
			var members = _context.Member.OrderBy(x => x.Pid)
			.Select(m => new MemberVM 
			{	
				Id = m.ID,
				Pid = m.Pid,
				NAME = m.NAME,
				INFO = m.INFO,
				CreateDt = m.CreateDt	
			}).ToList();

			return members;
		}

		public MemberVM GetMember(Guid? id) 
		{
			var member = _context.Member.Where(x => x.ID == id)
				.Select(m => new MemberVM
				{
					Id = m.ID,
					Pid = m.Pid,
					NAME = m.NAME,
					INFO = m.INFO,
					CreateDt = m.CreateDt
				}).FirstOrDefault();
			return member;
		}

		public MemberVM AddMember(MemberVM member) 
		{
			var newMember = new Member
			{
				ID = Guid.NewGuid(),
				NAME = member.NAME,
				INFO = member.INFO,
				CreateDt = DateTime.Now
			};

			_context.Member.Add(newMember);
			_context.SaveChanges();
			return member;
		}


		public MemberVM UpdateMember(MemberVM member) 
		{
			var newMember = _context.Member.Find(member.Id);
			if (newMember != null)
			{
				newMember.NAME = member.NAME;
				newMember.INFO = member.INFO;

				_context.SaveChanges();
			}
			
			return member;
		}



		public MemberVM DeleteMember(MemberVM member) 
		{
			var deleteMember = _context.Member.Find(member.Id);
			if (deleteMember == null)
			{
				return null;
			}

			_context.Member.Remove(deleteMember);
			_context.SaveChanges();
			return member;
		}

	}
}
