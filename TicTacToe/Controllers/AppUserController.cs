using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TicTacToe.DB;
using TicTacToe.DB.Contexts;
using TicTacToe.ViewModel;

namespace TicTacToe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppUserController : ControllerBase
    {
        private readonly tictakContexts dbtictakContexts;
        private readonly IConfiguration _configuration;
        public AppUserController(tictakContexts dbtictakContexts, IConfiguration _configuration)
        {
            this._configuration = _configuration;
            this.dbtictakContexts = dbtictakContexts;
        }
        /// <summary>
        /// Login
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(UserLoginViewModel model)
        {
            try
            {
                var _result =  dbtictakContexts.Tbl_User.Where(x=>x.Email == model.email && x.Password == model.password).FirstOrDefault();
                if (_result.Email != null)
                {
                    //create claims
                    var Claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier,_result.UserName),
                        new Claim(ClaimTypes.Name,_result.Id.ToString()),
                        new Claim(ClaimTypes.Email,_result.Email),
                        new Claim(ClaimTypes.MobilePhone,_result.Email),
                        new Claim(ClaimTypes.Locality,_result.Location),
                        new Claim(ClaimTypes.Role,_result.Role),

                    };
                    var token = new JwtSecurityToken
                        (
                        issuer: _configuration["Jwt:Issuer"],
                        audience: _configuration["Jwt:Audience"],
                        claims: Claims,
                        expires: DateTime.UtcNow.AddDays(1),
                        notBefore: DateTime.UtcNow,
                        signingCredentials: new SigningCredentials(
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                            SecurityAlgorithms.HmacSha256)

                        );
                    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                    return Ok(new { status = true, token = tokenString, UserData = _result });
                }
                return BadRequest(new { Status = false, message = "This User Not Found" });

            }
            catch
            {

                return BadRequest(new { Status = false, message = "Error" });
            }
        }


        /// <summary>
        /// Create User
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Registration")]
        public async Task<IActionResult> Registration(UserRegistrationModel model)
        {
            try
            {
                if (!(dbtictakContexts.Tbl_User.Where(x => x.Email == model.email).Any()))
                {
                    Tbl_User obj = new Tbl_User()
                    {
                        Email = model.email,
                        Password = model.password,
                        PhoneNumber = model.phoneNumber,
                        Location = model.location,
                        UserName = model.userName,
                        Role = "User",
                        IsActive = true,
                        IsDeleted = false,
                    };
                    dbtictakContexts.Add(obj);
                    await dbtictakContexts.SaveChangesAsync();
                   
                    return Ok(new { Status = true, message = "Created" });
                }

                return Ok(new { Status = false, message = "Already Exsit" });
              


            }
            catch
            {

                return BadRequest(new { Status = false, message = "Error" });
            }
        }

        [HttpPost]
        [Route("GetAllPlayers")]
        public async Task<IActionResult> GetAllPlayers()
        {
            try
            {
                
                  var Players =   dbtictakContexts.Tbl_User.Select(x => 
                     new Players
                     {

                        id = x.Id,
                        phoneNumber = x.PhoneNumber,
                        location = x.Location,
                        userName = x.UserName,
                       
                    }).ToList();
                   
                    return Ok(new { Status = true, message = "Succesfull", players= Players });
              



            }
            catch
            {

                return BadRequest(new { Status = false, message = "Error" });
            }
        }

        
    }

   


    public class Players
    {

        public long id { get; set; }
        public string? userName { get; set; }
        public string? phoneNumber { get; set; }
        public string? location { get; set; }
       


    }
}
