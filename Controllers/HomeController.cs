using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TEST.Models;
using TEST.Services;

namespace TEST.Controllers
{
	[Authorize]
	public class HomeController : Controller
    {
		private readonly MemberBL _MemberBL;

		public HomeController(MemberBL MemberBL)
		{
			_MemberBL = MemberBL;
		}

		[AllowAnonymous]
		public IActionResult Index()
        {
            return View();
        }

		public IActionResult List()
		{
            var ListAll = _MemberBL.GetMemberList();

            if (ListAll == null)
            {
                return NotFound();
            }
            else
            {
				return View(ListAll);
			}
		}


		public IActionResult Details(int? id)
		{
            if (id == null)
            {
				return BadRequest();
			}

            var member = _MemberBL.GetMember(id);

			if (member == null)
            {
                return NotFound();
			}else
            {
                return View(member);
			}
		}


		public IActionResult Create()
		{
			return View();
		}

        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Member member)
        {
            if (member != null && ModelState.IsValid)
            {
                _MemberBL.AddMember(member);
				return RedirectToAction("List", "Home");
			}
            return View(member);
		}


		public IActionResult Edit(int? id)
		{
			if (id == null)
			{
				return BadRequest();
			}

            var member = _MemberBL.GetMember(id);
            if (member == null)
            {
                return NotFound();
            }
            else
            {
                return View(member);
			}	
		}

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Member member)
        {
            if (member == null)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var memberEdit = _MemberBL.GetMember(member.Pid);
                if (memberEdit == null)
                {
                    return NotFound();
                }
                else
                {
                    memberEdit.NAME = member.NAME;
                    memberEdit.INFO = member.INFO;
                    _MemberBL.UpdateMember(memberEdit);
                    return RedirectToAction("List", "Home");
                }
            }
            else
            {
                return View(member);
            }
        }


		public IActionResult Delete(int? id)
		{
			if (id == null)
			{
				return BadRequest();
			}

			var member = _MemberBL.GetMember(id);
			if (member == null)
			{
				return NotFound();
			}
			else
			{
				return View(member);
			}
		}

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirm(int? id)
        {
            if (ModelState.IsValid)
            {
                var member = _MemberBL.GetMember(id);
                _MemberBL.DeleteMember(member);

				return RedirectToAction("List", "Home");

            }
            else
            {
                ModelState.AddModelError("Value", "¦Û­q¿ù»~°T®§");
                return View();
            }
        
        }



		public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
