using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitSync.Data;
using SplitSync.Entities;
using SplitSync.Models;
using SplitSync.Services;
using System.Drawing;
using System.Security.Claims;
using System.Security.Cryptography;

namespace SplitSync.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailService emailService;

        public AccountController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            this.emailService = emailService;
        }

        private async Task SignIn(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? user.Email)
            };
            var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(id),
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) });
        }

        [HttpGet, AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View(new AccountLoginViewModel());
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AccountLoginViewModel dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Identifier) || string.IsNullOrWhiteSpace(dto.Password))
            {
                dto.ErrorMessage = "Username/email i lozinka su obavezni.";
                return View(dto);
            }

            var identifierToLower = dto.Identifier.Trim().ToLowerInvariant();

            var user = await _context.Users.FirstOrDefaultAsync(u =>
                (u.Username != null && u.Username.ToLower() == identifierToLower) || u.Email.ToLower() == identifierToLower);

            var hasher = new PasswordHasher();
            if (user == null || string.IsNullOrEmpty(user.PasswordHash) || !hasher.Verify(dto.Password, user.PasswordHash))
            {
                dto.ErrorMessage = "Pogrešni kredencijali.";
                return View(dto);
            }

            if (!user.IsEmailVerified)
            {
                dto.ErrorMessage = "Email nije verifikovan.";
                return View(dto);
            }

            await SignIn(user);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet, AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View(new AccountRegisterViewModel());
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AccountRegisterViewModel dto)
        {
            dto.Email = dto.Email?.Trim() ?? "";
            dto.Username = dto.Username?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.ConfirmPassword))
            {
                dto.ErrorMessage = "Sva polja su obavezna.";
                return View(dto);
            }

            if (dto.Password != dto.ConfirmPassword)
            {
                dto.ErrorMessage = "Lozinke se ne poklapaju.";
                dto.Password = "";
                dto.ConfirmPassword = "";
                return View(dto);
            }

            var emailToLower = dto.Email.ToLowerInvariant();
            var usernameToLower = dto.Username.ToLowerInvariant();

            if (await _context.Users.AnyAsync(u => u.Email == emailToLower))
            {
                dto.ErrorMessage = "Email je već registrovan.";
                return View(dto);
            }

            if (dto.Username.Length > 20)
            {
                dto.ErrorMessage = "Korisničko ime ne sme da bude duže od 20 karaktera.";
                return View(dto);
            }

            if (await _context.Users.AnyAsync(u => u.Username != null && u.Username.ToLower() == usernameToLower))
            {
                dto.ErrorMessage = "Korisničko ime je zauzeto.";
                return View(dto);
            }

            var existing = await _context.EmailConfirmations.FirstOrDefaultAsync(x => x.Email == emailToLower);

            var code = GenerateCode(6);
            var hasher = new PasswordHasher();
            var hash = hasher.Hash(dto.Password);

            if (existing == null)
            {
                existing = new EmailConfirmation
                {
                    Email = emailToLower,
                    Username = usernameToLower,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    PasswordHash = hash,
                    Code = code,
                    ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15)
                };
                _context.EmailConfirmations.Add(existing);
            }
            else
            {
                existing.Username = usernameToLower;
                existing.FirstName = dto.FirstName;
                existing.LastName = dto.LastName;
                existing.PasswordHash = hash;
                existing.Code = code;
                existing.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15);
            }

            await _context.SaveChangesAsync();

            // Email sa kodom
            var html = $@"
                <!doctype html>
                <html lang='sr'>
                <body style='margin:0;padding:0;background:#eaf1ff;'>
                <table role='presentation' width='100%' cellpadding='0' cellspacing='0' style='background:#eaf1ff;padding:24px 0;'>
                    <tr>
                    <td align='center'>
                        <table role='presentation' width='100%' cellpadding='0' cellspacing='0' style='max-width:560px;background:#ffffff;border-radius:14px;box-shadow:0 4px 18px rgba(37,99,235,.15);overflow:hidden;'>
                        <tr>
                            <td style='height:6px;background:#2563eb;'></td>
                        </tr>
                        <tr>
                            <td style='padding:28px 28px 8px 28px;font-family:Segoe UI,Roboto,Helvetica,Arial,sans-serif;color:#0f172a;'>
                            <h1 style='margin:0 0 8px 0;font-size:22px;line-height:1.25;font-weight:700;color:#111827;'>SplitSync verifikacija</h1>
                            <p style='margin:0 0 18px 0;font-size:14px;color:#374151;'>Zdravo {dto.FirstName} {dto.LastName},</p>
                            <p style='margin:0 0 10px 0;font-size:15px;color:#111827;'>Vaš verifikacioni kod je:</p>
                            </td>
                        </tr>
                        <tr>
                            <td style='padding:0 28px 4px 28px;'>
                            <div style='text-align:center;padding:14px 18px;border:1px dashed #2563eb;border-radius:10px;background:#f3f6ff;font-family:SFMono-Regular,Consolas,Monaco,monospace;font-size:28px;letter-spacing:6px;font-weight:700;color:#111827;'>
                                {code}
                            </div>
                            </td>
                        </tr>
                        <tr>
                            <td style='padding:12px 28px 24px 28px;font-family:Segoe UI,Roboto,Helvetica,Arial,sans-serif;color:#374151;'>
                            <p style='margin:12px 0 0 0;font-size:14px;'>Kod važi 15 minuta.</p>
                            </td>
                        </tr>
                        <tr>
                            <td style='padding:16px 28px 28px 28px;font-family:Segoe UI,Roboto,Helvetica,Arial,sans-serif;font-size:12px;color:#6b7280;text-align:center;'>
                            Ako niste vi tražili verifikaciju, slobodno ignorišite ovu poruku.
                            </td>
                        </tr>
                        </table>
                    </td>
                    </tr>
                </table>
                </body>
                </html>";
            await emailService.SendEmail(emailToLower, "Verifikacija email adrese", html);

            return RedirectToAction(nameof(Verify), new { email = emailToLower });
        }

        [HttpGet, AllowAnonymous]
        public IActionResult Verify(string email)
        {
            return View(new AccountVerifyViewModel
            {
                Email = email
            });
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(AccountVerifyViewModel dto)
        {
            var emailLower = (dto.Email ?? "").Trim().ToLowerInvariant();

            var pending = await _context.EmailConfirmations
                .FirstOrDefaultAsync(x => x.Email == emailLower &&
                                            x.Code == dto.Code &&
                                            x.ExpiresAtUtc >= DateTime.UtcNow);

            if (pending == null)
            {
                dto.ErrorMessage = "Nevažeći ili istekao kod.";
                return View(dto);
            }

            if (await _context.Users.AnyAsync(u => u.Username != null && u.Username.ToLower() == pending.Username.ToLower()))
            {
                dto.ErrorMessage = "Korisničko ime je u međuvremenu postalo zauzeto. Vratite se na registraciju i izaberite drugo.";
                return View(dto);
            }

            var user = new User
            {
                Email = pending.Email,
                Username = pending.Username,
                FirstName = pending.FirstName,
                LastName = pending.LastName,
                PasswordHash = pending.PasswordHash,
                IsEmailVerified = true,
                CreatedAtUtc = DateTime.UtcNow
            };
            _context.Users.Add(user);

            var emailConfirmations = _context.EmailConfirmations.Where(x => x.Email == pending.Email);
            _context.EmailConfirmations.RemoveRange(emailConfirmations);

            await _context.SaveChangesAsync();
            await SignIn(user);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet, AllowAnonymous]
        public IActionResult GoogleUsername()
        {
            var sub = HttpContext.Session.GetString("google_sub");
            var email = HttpContext.Session.GetString("google_email");
            if (string.IsNullOrEmpty(sub) || string.IsNullOrEmpty(email))
                return RedirectToAction(nameof(Login));
            return View(new AccountGoogleUsernameViewModel());
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> GoogleUsername(AccountGoogleUsernameViewModel dto)
        {
            var sub = HttpContext.Session.GetString("google_sub");
            var email = HttpContext.Session.GetString("google_email");
            var firstName = HttpContext.Session.GetString("google_given") ?? "";
            var lastName = HttpContext.Session.GetString("google_family") ?? "";

            if (string.IsNullOrEmpty(sub) || string.IsNullOrEmpty(email))
                return RedirectToAction(nameof(Login));

            if (!string.IsNullOrWhiteSpace(dto.Username) && dto.Username.Length > 20)
            {
                dto.ErrorMessage = "Korisničko ime ne sme da bude duže od 20 karaktera.";
                return View(dto);
            }

            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            {
                dto.ErrorMessage = "Korisničko ime je zauzeto.";
                return View(dto);
            }

            var user = new User
            {
                Email = email.ToLowerInvariant(),
                Username = dto.Username.ToLowerInvariant(),
                FirstName = firstName,
                LastName = lastName,
                PasswordHash = null,
                IsEmailVerified = true,
                GoogleSubject = sub,
                CreatedAtUtc = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove("google_sub");
            HttpContext.Session.Remove("google_email");
            HttpContext.Session.Remove("google_given");
            HttpContext.Session.Remove("google_family");

            await SignIn(user);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        private static string GenerateCode(int digits)
        {
            string code = "";
            for (int i = 0; i < digits; i++)
                code += RandomNumberGenerator.GetInt32(0, 10).ToString();
            return code;
        }

        [HttpGet, AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel dto)
        {
            var emailLower = (dto.Email ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(emailLower))
            {
                dto.ErrorMessage = "Unesite email.";
                return View(dto);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailLower);
            if (user == null)
            {
                return View(new ForgotPasswordViewModel { ErrorMessage = "Ne postoji nalog sa tom email adresom." });
            }

            var pr = await _context.PasswordResets.FirstOrDefaultAsync(x => x.Email == emailLower);

            string code;
            if (pr == null)
            {
                code = GenerateCode(6);
                pr = new PasswordReset
                {
                    Email = emailLower,
                    Code = code,
                    ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15)
                };
                _context.PasswordResets.Add(pr);
            }
            else
            {
                code = pr.Code;
                pr.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15);
            }

            await _context.SaveChangesAsync();
            var html = $@"
                <!doctype html>
                <html lang='sr'>
                  <body style='margin:0;padding:0;background:#eaf1ff;'>
                    <table role='presentation' width='100%' cellpadding='0' cellspacing='0' style='background:#eaf1ff;padding:24px 0;'>
                      <tr>
                        <td align='center'>
                          <table role='presentation' width='100%' cellpadding='0' cellspacing='0' style='max-width:560px;background:#ffffff;border-radius:14px;box-shadow:0 4px 18px rgba(37,99,235,.15);overflow:hidden;'>
                            <tr>
                              <td style='height:6px;background:#2563eb;'></td>
                            </tr>
                            <tr>
                              <td style='padding:28px 28px 8px 28px;font-family:Segoe UI,Roboto,Helvetica,Arial,sans-serif;color:#0f172a;'>
                                <h1 style='margin:0 0 10px 0;font-size:22px;line-height:1.25;font-weight:700;color:#111827;'>
                                  Reset lozinke — SplitSync
                                </h1>
                                <p style='margin:0 0 14px 0;font-size:15px;color:#111827;'>
                                  Vaš verifikacioni kod za reset lozinke:
                                </p>
                              </td>
                            </tr>
                            <tr>
                              <td style='padding:0 28px 4px 28px;'>
                                <div style='text-align:center;padding:14px 18px;border:1px dashed #2563eb;border-radius:10px;background:#f3f6ff;font-family:SFMono-Regular,Consolas,Monaco,monospace;font-size:28px;letter-spacing:6px;font-weight:700;color:#111827;'>
                                  {code}
                                </div>
                              </td>
                            </tr>
                            <tr>
                              <td style='padding:12px 28px 24px 28px;font-family:Segoe UI,Roboto,Helvetica,Arial,sans-serif;color:#374151;'>
                                <p style='margin:12px 0 0 0;font-size:14px;'>Kod važi 15 minuta.</p>
                              </td>
                            </tr>
                            <tr>
                              <td style='padding:16px 28px 28px 28px;font-family:Segoe UI,Roboto,Helvetica,Arial,sans-serif;font-size:12px;color:#6b7280;text-align:center;'>
                                Ako niste tražili reset lozinke, ignorišite ovu poruku.
                              </td>
                            </tr>
                          </table>
                        </td>
                      </tr>
                    </table>
                  </body>
                </html>";
            await emailService.SendEmail(emailLower, "Reset lozinke", html);

            return RedirectToAction(nameof(ResetPassword), new { email = emailLower });
        }

        [HttpGet, AllowAnonymous]
        public IActionResult ResetPassword(string email)
        {
            return View(new ResetPasswordViewModel { Email = email });
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel dto)
        {
            var emailLower = (dto.Email ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(emailLower)
                || string.IsNullOrWhiteSpace(dto.Code)
                || string.IsNullOrWhiteSpace(dto.Password)
                || string.IsNullOrWhiteSpace(dto.ConfirmPassword))
            {
                dto.ErrorMessage = "Sva polja su obavezna.";
                return View(dto);
            }

            if (dto.Password != dto.ConfirmPassword)
            {
                dto.ErrorMessage = "Lozinke se ne poklapaju.";
                dto.Password = dto.ConfirmPassword = "";
                return View(dto);
            }

            var pr = await _context.PasswordResets
                .FirstOrDefaultAsync(x => x.Email == emailLower && x.Code == dto.Code && x.ExpiresAtUtc >= DateTime.UtcNow);

            if (pr == null)
            {
                dto.ErrorMessage = "Nevažeći ili istekao kod.";
                return View(dto);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailLower);
            if (user == null)
            {
                dto.ErrorMessage = "Nalog nije pronađen.";
                return View(dto);
            }

            var hasher = new PasswordHasher();
            user.PasswordHash = hasher.Hash(dto.Password);

            var resets = _context.PasswordResets.Where(x => x.Email == emailLower);
            _context.PasswordResets.RemoveRange(resets);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Login));
        }

        [Authorize, HttpGet]
        public async Task<IActionResult> Edit()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(idStr, out var userId))
                return RedirectToAction(nameof(Login));

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return RedirectToAction(nameof(Login));

            string? base64Profilna = null;
            if (user.Slika != null && user.Slika.Length > 0)
                base64Profilna = Convert.ToBase64String(user.Slika);

            var vm = new AccountEditViewModel
            {
                Username = user.Username ?? "",
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                Email = user.Email,
                HasPassword = !string.IsNullOrEmpty(user.PasswordHash),
                OldPictureBase64 = base64Profilna
            };
            return View(vm);
        }


        [Authorize, HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AccountEditViewModel dto)
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(idStr, out var userId))
                return RedirectToAction(nameof(Login));

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return RedirectToAction(nameof(Login));


            var newUsername = dto.Username?.Trim();
            var newFirst = dto.FirstName?.Trim();
            var newLast = dto.LastName?.Trim();


            if (!string.IsNullOrWhiteSpace(newUsername) &&
                !string.Equals(user.Username, newUsername, StringComparison.OrdinalIgnoreCase))
            {
                if (newUsername.Length > 20)
                {
                    dto.ErrorMessage = "Korisničko ime ne sme da bude duže od 20 karaktera.";
                    dto.Email = user.Email;
                    dto.HasPassword = !string.IsNullOrEmpty(user.PasswordHash);
                    return View(dto);
                }

                var taken = await _context.Users.AnyAsync(u =>
                    u.Id != user.Id &&
                    u.Username != null &&
                    u.Username.ToLower() == newUsername.ToLower());

                if (taken)
                {
                    dto.ErrorMessage = "Korisničko ime je zauzeto.";
                    dto.Email = user.Email;
                    dto.HasPassword = !string.IsNullOrEmpty(user.PasswordHash);
                    return View(dto);
                }
                user.Username = newUsername;
            }

            user.FirstName = newFirst;
            user.LastName = newLast;


            var wantsPwdChange = !string.IsNullOrWhiteSpace(dto.NewPassword) ||
                                 !string.IsNullOrWhiteSpace(dto.ConfirmNewPassword);

            if (wantsPwdChange)
            {
                if (dto.NewPassword != dto.ConfirmNewPassword)
                {
                    dto.ErrorMessage = "Lozinke se ne poklapaju.";
                    dto.Email = user.Email;
                    dto.HasPassword = !string.IsNullOrEmpty(user.PasswordHash);
                    dto.CurrentPassword = "";
                    return View(dto);
                }

                var hasher = new PasswordHasher();

                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    if (string.IsNullOrEmpty(dto.CurrentPassword) ||
                        !hasher.Verify(dto.CurrentPassword, user.PasswordHash))
                    {
                        dto.ErrorMessage = "Trenutna lozinka nije tačna.";
                        dto.Email = user.Email;
                        dto.HasPassword = true;
                        dto.CurrentPassword = "";
                        return View(dto);
                    }
                }

                user.PasswordHash = hasher.Hash(dto.NewPassword!);
            }

            if(dto.NovaSlika != null && dto.NovaSlika.Length > 0)
            {
                var extension = Path.GetExtension(dto.NovaSlika.FileName);
                user.SlikaExtension = extension;

                using var ms = new MemoryStream();
                await dto.NovaSlika.CopyToAsync(ms);
                user.Slika = ms.ToArray();
            }

            await _context.SaveChangesAsync();


            dto.SuccessMessage = "Profil je uspešno ažuriran.";
            dto.Email = user.Email;
            dto.HasPassword = !string.IsNullOrEmpty(user.PasswordHash);
            if (user.Slika != null && user.Slika.Length > 0)
                dto.OldPictureBase64 = Convert.ToBase64String(user.Slika);
            return View(dto);
        }
    }
}

