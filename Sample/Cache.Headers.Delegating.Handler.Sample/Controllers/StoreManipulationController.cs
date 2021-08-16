using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marvin.Cache.Headers;
using Marvin.Cache.Headers.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Cache.Headers.Delegating.Handler.Sample.Controllers
{
    /// <summary>
    /// <see href="https://github.com/KevinDockx/HttpCacheHeaders/blob/7c7f0fe94b70e4273f0a8011c8daf78adb220c13/sample/Marvin.Cache.Headers.Sample/Controllers/StoreManipulationController.cs"/>
    /// </summary>
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
            //simulating some work getting done
            Thread.Sleep(100);
            return new[] { "value1", "value2" };
        }
         
        [HttpGet("{id}")] 
        public async Task<string> Get(int id)
        { 
            //simulating some work getting done
            Thread.Sleep(100);
            return "value";
        } 

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] string value)
        {
            // code to post omitted

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

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] string value)
        {
            // code to update omitted

            // remove items based on the current path

            // 1) find the keys related to the current resource path
            var matchingKeysByPath = await _storeKeyAccessor.FindByCurrentResourcePath();
            if (matchingKeysByPath.Any())
            {
                // 2) mark them (often just one) for invalidation
                await _validatorValueInvalidator.MarkForInvalidation(matchingKeysByPath);
            }

            return NoContent();
        }
    }
}