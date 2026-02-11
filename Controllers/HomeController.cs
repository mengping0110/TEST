using System;
using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using TEST.Models;
using TEST.Services;

namespace TEST.Controllers
{
	[Authorize]
	public class HomeController : Controller
    {
		private readonly MemberBL _MemberBL;
		private readonly IWebHostEnvironment _hostingEnvironment;

		public HomeController(MemberBL MemberBL, IWebHostEnvironment hostingEnvironment)
		{
			_MemberBL = MemberBL;
			_hostingEnvironment = hostingEnvironment;
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


		public IActionResult Details(Guid id)
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
        public IActionResult Create(MemberVM member)
        {
            if (member != null && ModelState.IsValid)
            {
                _MemberBL.AddMember(member);
				return RedirectToAction("List", "Home");
			}
            return View(member);
		}


		public IActionResult Edit(Guid? id)
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
        public IActionResult Edit(MemberVM member)
        {
            if (member == null)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var memberEdit = _MemberBL.GetMember(member.Id);
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


		public IActionResult Delete(Guid? id)
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
        public IActionResult DeleteConfirm(Guid? id)
        {
            if (ModelState.IsValid)
            {
                var member = _MemberBL.GetMember(id);
                _MemberBL.DeleteMember(member);

				return RedirectToAction("List", "Home");

            }
            else
            {
                ModelState.AddModelError("Value", "自訂錯誤訊息");
                return View();
            }
        
        }

		public IActionResult Upload()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Upload(IFormFile FileUpload_FileName) 
		{
			try
			{
				if (FileUpload_FileName != null && FileUpload_FileName.Length > 0)
				{

					// 定義黑名單 (副檔名)
					var deniedExtensions = new[] { ".aspx", ".jpg", ".pdf" };
					string fileName = Path.GetFileName(FileUpload_FileName.FileName);
					string extension = Path.GetExtension(fileName).ToLower();

					// 定義黑名單 (MIME Type)
					var deniedMimeTypes = new[] {
						"application/x-aspx",
						"image/jpeg",
						"image/pjpeg",
						"application/pdf"
					};
					string contentType = FileUpload_FileName.ContentType.ToLower();

					// 檢查是否在黑名單內
					if (deniedExtensions.Contains(extension) || deniedMimeTypes.Contains(contentType))
					{
						ViewBag.Message = $"錯誤：不允許上傳 {extension} 或類型為 {contentType} 的檔案。";
						return View();
					}

					string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads");

					string filePath = Path.Combine(uploadsFolder, fileName);

					if (!Directory.Exists(uploadsFolder))
					{
						Directory.CreateDirectory(uploadsFolder);
					}

					using (var fileStream = new FileStream(filePath, FileMode.Create))
					{
						await FileUpload_FileName.CopyToAsync(fileStream);
					}

					ViewBag.Message = "檔案上傳成功";
				}
				else
				{
					ViewBag.Message = "請選擇檔案";
				}
			}
			catch (Exception ex)
			{
				ViewBag.Message = "檔案上傳失敗：" + ex.Message;
			}

			return View();
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
