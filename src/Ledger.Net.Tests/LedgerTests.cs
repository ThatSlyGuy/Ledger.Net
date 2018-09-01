using Hid.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Ledger.Net.Tests
{
    public class LedgerTests
    {
        public static VendorProductIds[] WellKnownLedgerWallets = new VendorProductIds[]
        {
            new VendorProductIds(0x2c97),
            new VendorProductIds(0x2581, 0x3b7c)
        };

        static readonly UsageSpecification[] _UsageSpecification = new[] { new UsageSpecification(0xffa0, 0x01) };

        //[Fact]
        //public async Task GetAddressAnyBitcoinApp()
        //{
        //    var ledgerManager = await GetLedger();

        //    await ledgerManager.SetCoinNumber();

        //    var address = await ledgerManager.GetAddressAsync(0, false, 0, false);
        //}

        //[Fact]
        //public async Task GetAddress()
        //{
        //    var ledgerManager = await GetLedger();

        //    var address = await ledgerManager.GetAddressAsync(0, false, 0, false);
        //    if (address == null)
        //    {
        //        throw new Exception("Address not returned");
        //    }
        //}

        [Fact]
        public async Task SignEthereumTransaction()
        {
            var ledgerManager = await GetLedger();
            ledgerManager.SetCoinNumber(60);

            var response = await ledgerManager.EthSignTransactionAsync("0", "3b9aca00", "5208", "689c56aef474df92d44a1b70850f808488f9769c", "de0b6b3a7640000", "", "4");
            Assert.True(response.SignatureV == 4);
        }

        [Fact]
        public async Task GetEthereumAddress()
        {
            var ledgerManager = await GetLedger();

            ledgerManager.SetCoinNumber(60);
            var address = await ledgerManager.GetAddressAsync(0, 0);

            if (address == null)
            {
                throw new Exception("Address not returned");
            }
        }

        private static async Task<LedgerManager> GetLedger()
        {
            var devices = new List<DeviceInformation>();

            var collection = WindowsHidDevice.GetConnectedDeviceInformations();

            foreach (var ids in WellKnownLedgerWallets)
            {
                if (ids.ProductId == null)
                    devices.AddRange(collection.Where(c => c.VendorId == ids.VendorId));
                else
                    devices.AddRange(collection.Where(c => c.VendorId == ids.VendorId && c.ProductId == ids.ProductId));
            }

            var retVal = devices
                .FirstOrDefault(d =>
                _UsageSpecification == null ||
                _UsageSpecification.Length == 0 ||
                _UsageSpecification.Any(u => d.UsagePage == u.UsagePage && d.Usage == u.Usage));

            var ledgerHidDevice = new WindowsHidDevice(retVal);
            await ledgerHidDevice.InitializeAsync();
            var ledgerManager = new LedgerManager(ledgerHidDevice);
            return ledgerManager;
        }
    }

    public class VendorProductIds
    {
        public VendorProductIds(int vendorId)
        {
            VendorId = vendorId;
        }
        public VendorProductIds(int vendorId, int? productId)
        {
            VendorId = vendorId;
            ProductId = productId;
        }
        public int VendorId
        {
            get;
        }
        public int? ProductId
        {
            get;
        }
    }

    public class UsageSpecification
    {
        public UsageSpecification(ushort usagePage, ushort usage)
        {
            UsagePage = usagePage;
            Usage = usage;
        }

        public ushort Usage
        {
            get;
        }
        public ushort UsagePage
        {
            get;
        }
    }
}
