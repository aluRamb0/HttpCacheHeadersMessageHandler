using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marvin.Cache.Headers;
using Marvin.Cache.Headers.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Http.Cache.Headers.MessageHandler.Sample.Controllers
{
    
    /// <see href="https://github.com/KevinDockx/HttpCacheHeaders/blob/7c7f0fe94b70e4273f0a8011c8daf78adb220c13/sample/Marvin.Cache.Headers.Sample/Controllers/StoreManipulationController.cs"/>
    [ApiController]
    [Route("[controller]")]
    [Route("api/storemanipulation")]
    public class StoreManipulationController : Controller
    {
        private readonly IValidatorValueInvalidator _validatorValueInvalidator;
        private readonly IStoreKeyAccessor _storeKeyAccessor;

        public StoreManipulationController(
            IValidatorValueInvalidator validatorValueInvalidator,
            IStoreKeyAccessor storeKeyAccessor)
        {          
            _validatorValueInvalidator = validatorValueInvalidator 
                ?? throw new ArgumentNullException(nameof(validatorValueInvalidator));
            _storeKeyAccessor = storeKeyAccessor 
                ?? throw new ArgumentNullException(nameof(storeKeyAccessor));
        }
 
        [HttpGet] 
        public IEnumerable<string> Get()
        {
            //assume some work
            Thread.Sleep(100);
            return new[] { "value1", "value2" };
        }
         
        [HttpGet("{id}")] 
        public async Task<string> Get(int id)
        {
            //assume some work
            Thread.Sleep(100);
            return "value";
        } 

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] string value)
        {
            //assume lots of work
            Thread.Sleep(100);
            
            // remove all items matching part of a resource path

            // 1) find the keys matching a certain string
            var matchingKeysByStringMatch = await _storeKeyAccessor.FindByKeyPart("api/storemanipulation");
            if (matchingKeysByStringMatch.Any())
            {
                // 2) mark all matches for invalidation
                await _validatorValueInvalidator.MarkForInvalidation(matchingKeysByStringMatch);
            }

            return NoContent();
        }
    }
}