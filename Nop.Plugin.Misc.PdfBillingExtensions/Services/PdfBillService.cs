using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Html;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.PdfBillingExtensions.Domain;
using Nop.Plugin.Misc.PdfBillingExtensions.Models;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Services.Vendors;

namespace Nop.Plugin.Misc.PdfBillingExtensions.Services
{
    public class PdfBillService : PdfService
    {
        private readonly AddressSettings _addressSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IMeasureService _measureService;
        private readonly INopFileProvider _fileProvider;
        private readonly IOrderService _orderService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPaymentService _paymentService;
        private readonly IPictureService _pictureService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly MeasureSettings _measureSettings;
        private readonly PdfSettings _pdfSettings;
        private readonly TaxSettings _taxSettings;
        private readonly VendorSettings _vendorSettings;
        IRepository<PdfBillRecord> _pdfBillsRepository;

        public PdfBillService(
            AddressSettings addressSettings, 
            CatalogSettings catalogSettings, 
            CurrencySettings currencySettings, 
            IAddressAttributeFormatter addressAttributeFormatter, 
            ICurrencyService currencyService, 
            IDateTimeHelper dateTimeHelper, 
            ILanguageService languageService, 
            ILocalizationService localizationService, 
            IMeasureService measureService, 
            INopFileProvider fileProvider, 
            IOrderService orderService, 
            IPaymentPluginManager paymentPluginManager, 
            IPaymentService paymentService, 
            IPictureService pictureService, 
            IPriceFormatter priceFormatter, 
            IProductService productService, 
            ISettingService settingService, 
            IStoreContext storeContext, 
            IStoreService storeService, 
            IVendorService vendorService, 
            IWorkContext workContext, 
            MeasureSettings measureSettings, 
            PdfSettings pdfSettings, 
            TaxSettings taxSettings, 
            VendorSettings vendorSettings,
            IRepository<PdfBillRecord> pdfBillsRepository) 
            : base(addressSettings, catalogSettings, currencySettings, addressAttributeFormatter, currencyService, 
                dateTimeHelper, languageService, localizationService, measureService, fileProvider, orderService, 
                paymentPluginManager, paymentService, pictureService, priceFormatter, productService, settingService, 
                storeContext, storeService, vendorService, workContext, measureSettings, pdfSettings, taxSettings, vendorSettings)
        {
            _addressSettings = addressSettings;
            _catalogSettings = catalogSettings;
            _currencySettings = currencySettings;
            _addressAttributeFormatter = addressAttributeFormatter;
            _currencyService = currencyService;
            _dateTimeHelper = dateTimeHelper;
            _languageService = languageService;
            _localizationService = localizationService;
            _measureService = measureService;
            _fileProvider = fileProvider;
            _orderService = orderService;
            _paymentPluginManager = paymentPluginManager;
            _paymentService = paymentService;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _settingService = settingService;
            _storeContext = storeContext;
            _storeService = storeService;
            _vendorService = vendorService;
            _workContext = workContext;
            _measureSettings = measureSettings;
            _pdfSettings = pdfSettings;
            _taxSettings = taxSettings;
            _vendorSettings = vendorSettings;
            _pdfBillsRepository = pdfBillsRepository;
        }

        public override void PrintOrdersToPdf(Stream stream, IList<Order> orders, int languageId = 0, int vendorId = 0)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (orders == null)
                throw new ArgumentNullException(nameof(orders));

            var storeId = orders.FirstOrDefault()?.StoreId ?? 0;
            var billSettings = _settingService.LoadSetting<PdfExtendedSettings>(storeId);

            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.Letter;
            }

            var doc = new Document(pageSize);
            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.Black;
            var font = GetFont();
            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            var ordCount = orders.Count;
            var ordNum = 0;

            foreach (var order in orders)
            {
                var billData = GetOrInsertBillData(order.Id);

                if (billData == null)
                {
                    throw new Exception($"Could not create bill record for order {order.Id}");
                }

                //by default _pdfSettings contains settings for the current active store
                //and we need PdfSettings for the store which was used to place an order
                //so let's load it based on a store of the current order
                var pdfSettingsByStore = _settingService.LoadSetting<PdfSettings>(order.StoreId);

                var lang = _languageService.GetLanguageById(languageId == 0 ? order.CustomerLanguageId : languageId);
                if (lang == null || !lang.Published)
                    lang = _workContext.WorkingLanguage;

                //header
                PrintHeader(pdfSettingsByStore, billSettings, billData, lang, order, font, titleFont, doc);

                //addresses
                PrintAddresses(billSettings, vendorId, lang, titleFont, order, font, doc);

                //products
                PrintProducts(vendorId, lang, titleFont, doc, order, font, attributesFont);

                //totals
                PrintTotalsAndSignature(billSettings, lang, order, font, titleFont, doc);

                //footer
                PrintFooter(pdfSettingsByStore, pdfWriter, pageSize, lang, font);

                ordNum++;
                if (ordNum < ordCount)
                {
                    doc.NewPage();
                }
            }

            doc.Close();
        }

        private PdfBillRecord GetOrInsertBillData(int orderId)
        {
            PdfBillRecord result = null;

            var curentDate = DateTime.Today;                  
            var newRecord = new PdfBillRecord { OrderId = orderId, Date = DateTime.Now };
            int attempts = 0;
            Exception lastException = null;

            do
            {
                try
                {
                    result = _pdfBillsRepository.Table.Where(b => b.OrderId == orderId).FirstOrDefault();
                    if (result != null)
                    {
                        return result;
                    }

                    var numberOfBillsThisYear = _pdfBillsRepository.Table.Where(b => b.Date.Year == curentDate.Year).Count();
                    var billNumber =
                    newRecord.BillNumber = (curentDate.Year * 10 + numberOfBillsThisYear + 1);

                    attempts++;
                    _pdfBillsRepository.Insert(newRecord);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }

            } while (result != null || attempts < 5);

            if (result == null && lastException != null)
            {
                throw lastException;
            }

            return result;
        }

        private void PrintTotalsAndSignature(PdfExtendedSettings billSettings, Language lang, Order order, Font font, Font titleFont, Document doc)
        {
            //table
            var signatureAndTotalsTable = new PdfPTable(2)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };
            signatureAndTotalsTable.DefaultCell.Border = Rectangle.BOX;

            var border = Rectangle.NO_BORDER;

            // signature
            PrintSignature(billSettings, lang, font, signatureAndTotalsTable);

            //table
            var totalsTable = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };
            totalsTable.DefaultCell.Border = border;

            //order subtotal
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax &&
                !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
            {
                //including tax

                var orderSubtotalInclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                var orderSubtotalInclTaxStr = _priceFormatter.FormatPrice(orderSubtotalInclTaxInCustomerCurrency, true,
                    order.CustomerCurrencyCode, lang, true);

                var p = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.Sub-Total", lang.Id)} {orderSubtotalInclTaxStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = border;
                totalsTable.AddCell(p);
            }
            else
            {
                //excluding tax

                var orderSubtotalExclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                var orderSubtotalExclTaxStr = _priceFormatter.FormatPrice(orderSubtotalExclTaxInCustomerCurrency, true,
                    order.CustomerCurrencyCode, lang, false);

                var p = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.Sub-Total", lang.Id)} {orderSubtotalExclTaxStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = border;
                totalsTable.AddCell(p);
            }

            //discount (applied to order subtotal)
            if (order.OrderSubTotalDiscountExclTax > decimal.Zero)
            {
                //order subtotal
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax &&
                    !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
                {
                    //including tax

                    var orderSubTotalDiscountInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                    var orderSubTotalDiscountInCustomerCurrencyStr = _priceFormatter.FormatPrice(
                        -orderSubTotalDiscountInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, true);

                    var p = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.Discount", lang.Id)} {orderSubTotalDiscountInCustomerCurrencyStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = border;
                    totalsTable.AddCell(p);
                }
                else
                {
                    //excluding tax

                    var orderSubTotalDiscountExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                    var orderSubTotalDiscountInCustomerCurrencyStr = _priceFormatter.FormatPrice(
                        -orderSubTotalDiscountExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, false);

                    var p = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.Discount", lang.Id)} {orderSubTotalDiscountInCustomerCurrencyStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = border;
                    totalsTable.AddCell(p);
                }
            }

            //shipping
            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var orderShippingInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                    var orderShippingInclTaxStr = _priceFormatter.FormatShippingPrice(
                        orderShippingInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, true);

                    var p = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.Shipping", lang.Id)} {orderShippingInclTaxStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = border;
                    totalsTable.AddCell(p);
                }
                else
                {
                    //excluding tax
                    var orderShippingExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                    var orderShippingExclTaxStr = _priceFormatter.FormatShippingPrice(
                        orderShippingExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, false);

                    var p = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.Shipping", lang.Id)} {orderShippingExclTaxStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = border;
                    totalsTable.AddCell(p);
                }
            }

            //payment fee
            if (order.PaymentMethodAdditionalFeeExclTax > decimal.Zero)
            {
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var paymentMethodAdditionalFeeInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                    var paymentMethodAdditionalFeeInclTaxStr = _priceFormatter.FormatPaymentMethodAdditionalFee(
                        paymentMethodAdditionalFeeInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, true);

                    var p = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.PaymentMethodAdditionalFee", lang.Id)} {paymentMethodAdditionalFeeInclTaxStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = border;
                    totalsTable.AddCell(p);
                }
                else
                {
                    //excluding tax
                    var paymentMethodAdditionalFeeExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                    var paymentMethodAdditionalFeeExclTaxStr = _priceFormatter.FormatPaymentMethodAdditionalFee(
                        paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, false);

                    var p = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.PaymentMethodAdditionalFee", lang.Id)} {paymentMethodAdditionalFeeExclTaxStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = border;
                    totalsTable.AddCell(p);
                }
            }

            //tax
            var taxStr = string.Empty;
            var taxRates = new SortedDictionary<decimal, decimal>();
            bool displayTax;
            var displayTaxRates = true;
            if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                displayTax = false;
            }
            else
            {
                if (order.OrderTax == 0 && _taxSettings.HideZeroTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    taxRates = _orderService.ParseTaxRates(order, order.TaxRates);

                    displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                    displayTax = !displayTaxRates;

                    var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                    taxStr = _priceFormatter.FormatPrice(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                        false, lang);
                }
            }

            if (displayTax)
            {
                var p = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.Tax", lang.Id)} {taxStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = border;
                totalsTable.AddCell(p);
            }

            if (displayTaxRates)
            {
                foreach (var item in taxRates)
                {
                    var taxRate = string.Format(_localizationService.GetResource("PDFInvoice.TaxRate", lang.Id),
                        _priceFormatter.FormatTaxRate(item.Key));
                    var taxValue = _priceFormatter.FormatPrice(
                        _currencyService.ConvertCurrency(item.Value, order.CurrencyRate), true, order.CustomerCurrencyCode,
                        false, lang);

                    var p = GetPdfCell($"{taxRate} {taxValue}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = border;
                    totalsTable.AddCell(p);
                }
            }

            //discount (applied to order total)
            if (order.OrderDiscount > decimal.Zero)
            {
                var orderDiscountInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
                var orderDiscountInCustomerCurrencyStr = _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency,
                    true, order.CustomerCurrencyCode, false, lang);

                var p = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.Discount", lang.Id)} {orderDiscountInCustomerCurrencyStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = border;
                totalsTable.AddCell(p);
            }

            //gift cards
            foreach (var gcuh in order.GiftCardUsageHistory)
            {
                var gcTitle = string.Format(_localizationService.GetResource("PDFInvoice.GiftCardInfo", lang.Id),
                    gcuh.GiftCard.GiftCardCouponCode);
                var gcAmountStr = _priceFormatter.FormatPrice(
                    -_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate), true,
                    order.CustomerCurrencyCode, false, lang);

                var p = GetPdfCell($"{gcTitle} {gcAmountStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = border;
                totalsTable.AddCell(p);
            }

            //reward points
            if (order.RedeemedRewardPointsEntry != null)
            {
                var rpTitle = string.Format(_localizationService.GetResource("PDFInvoice.RewardPoints", lang.Id),
                    -order.RedeemedRewardPointsEntry.Points);
                var rpAmount = _priceFormatter.FormatPrice(
                    -_currencyService.ConvertCurrency(order.RedeemedRewardPointsEntry.UsedAmount, order.CurrencyRate),
                    true, order.CustomerCurrencyCode, false, lang);

                var p = GetPdfCell($"{rpTitle} {rpAmount}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = border;
                totalsTable.AddCell(p);
            }

            //order total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            var orderTotalStr = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, lang);

            var pTotal = GetPdfCell($"{_localizationService.GetResource("PDFInvoice.OrderTotal", lang.Id)} {orderTotalStr}", titleFont);
            pTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
            pTotal.Border = border;
            totalsTable.AddCell(pTotal);

            var parentTableCell = new PdfPCell { Border = border };
            parentTableCell.AddElement(totalsTable);

            signatureAndTotalsTable.AddCell(parentTableCell);

            doc.Add(signatureAndTotalsTable);
        }

        private void PrintSignature(PdfExtendedSettings billSettings, Language lang, Font font, PdfPTable signatureAndTotalsTable)
        {
            var signatureTable = new PdfPTable(2)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };
            signatureTable.DefaultCell.Border = Rectangle.NO_BORDER;

            var cell = new PdfPCell(new Phrase(string.Empty))
            {
                Border = Rectangle.NO_BORDER, 
                HorizontalAlignment = Element.ALIGN_LEFT,
                VerticalAlignment = Element.ALIGN_TOP
            };
            
            var text = $"{_localizationService.GetResource("Plugins.Misc.PdfBillingExtension.BillingDelegateName", lang.Id)}: {billSettings.BillingDelegateName}";
            cell.Phrase.Add(GetTextParagraph(text, string.Empty, font));
            cell.Phrase.Add(new Phrase(Environment.NewLine));
            if (!string.IsNullOrWhiteSpace(billSettings.BillingDelegateId))
            {
                text = $"{_localizationService.GetResource("Plugins.Misc.PdfBillingExtension.BillingDelegateId", lang.Id)}: {billSettings.BillingDelegateId}";
                cell.Phrase.Add(GetTextParagraph(text, string.Empty, font));
                cell.Phrase.Add(new Phrase(Environment.NewLine));
            }

            if (billSettings.SignaturePictureId > 0)
            {
                var signaturePicture = _pictureService.GetPictureById(billSettings.SignaturePictureId);
                if (signaturePicture != null)
                {
                    // print signature picture
                    text = $"{_localizationService.GetResource("Plugins.Misc.PdfBillingExtension.SignaturePictureId", lang.Id)}:";
                    cell.Phrase.Add(GetTextParagraph(text, string.Empty, font));
                    cell.Phrase.Add(new Phrase(Environment.NewLine));
                    cell.Phrase.Add(new Phrase(Environment.NewLine));
                    signatureTable.AddCell(cell);

                    var logoFilePath = _pictureService.GetThumbLocalPath(signaturePicture, 0, false);
                    var logo = Image.GetInstance(logoFilePath);
                    logo.Alignment = GetAlignment(lang, true);
                    logo.ScaleToFit(175f, 80f);

                    var cellSiganture = new PdfPCell { Border = Rectangle.NO_BORDER };
                    cellSiganture.AddElement(logo);
                    cellSiganture.HorizontalAlignment = Element.ALIGN_LEFT;
                    cellSiganture.VerticalAlignment = Element.ALIGN_BOTTOM;
                    signatureTable.AddCell(cellSiganture);
                }
                else
                {
                    signatureTable.AddCell(cell);
                }
            }
            else
            {
                signatureTable.AddCell(cell);
            }

            var parentTableCell = new PdfPCell { Border = Rectangle.NO_BORDER };
            parentTableCell.AddElement(signatureTable);

            signatureAndTotalsTable.AddCell(parentTableCell);
        }

        private void PrintHeader(PdfSettings pdfSettingsByStore, PdfExtendedSettings pdfBillSettings, PdfBillRecord billData,
        Language lang, Order order, Font font, Font titleFont, Document doc)
        {
            //logo
            var logoPicture = _pictureService.GetPictureById(pdfSettingsByStore.LogoPictureId);
            var logoExists = logoPicture != null;

            //header
            var headerTable = new PdfPTable(logoExists ? 2 : 1)
            {
                RunDirection = GetDirection(lang)
            };
            headerTable.DefaultCell.Border = Rectangle.NO_BORDER;

            var cellHeader = GetPdfCell(string.Format(_localizationService.GetResource("Plugins.Misc.PdfBillingExtension.Bill", lang.Id)), titleFont);
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            var text = $"{_localizationService.GetResource("Plugins.Misc.PdfBillingExtension.BillingSerialNumber", lang.Id)}: {pdfBillSettings.BillingSerialNumber}";
            cellHeader.Phrase.Add(GetTextParagraph(text, string.Empty, font));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            text = $"{_localizationService.GetResource("Plugins.Misc.PdfBillingExtension.BillingNumber", lang.Id)}: {billData.BillNumber}";
            cellHeader.Phrase.Add(GetTextParagraph(text, string.Empty, font));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(GetParagraph("PDFInvoice.OrderDate", lang, font, billData.Date.ToString("d", new CultureInfo(lang.LanguageCulture))));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.HorizontalAlignment = Element.ALIGN_LEFT;
            cellHeader.Border = Rectangle.NO_BORDER;

            headerTable.AddCell(cellHeader);

            if (logoExists)
                headerTable.SetWidths(lang.Rtl ? new[] { 0.2f, 0.8f } : new[] { 0.8f, 0.2f });
            headerTable.WidthPercentage = 100f;

            //logo               
            if (logoExists)
            {
                var logoFilePath = _pictureService.GetThumbLocalPath(logoPicture, 0, false);
                var logo = Image.GetInstance(logoFilePath);
                logo.Alignment = GetAlignment(lang, true);
                logo.ScaleToFit(65f, 65f);

                var cellLogo = new PdfPCell { Border = Rectangle.NO_BORDER };
                cellLogo.AddElement(logo);
                cellLogo.HorizontalAlignment = Element.ALIGN_RIGHT;
                cellLogo.VerticalAlignment = Element.ALIGN_TOP;
                headerTable.AddCell(cellLogo);
            }

            doc.Add(headerTable);
        }

        /// <summary>
        /// Print addresses
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="lang">Language</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="order">Order</param>
        /// <param name="font">Text font</param>
        /// <param name="doc">Document</param>
        private void PrintAddresses(PdfExtendedSettings billSettings, int vendorId, Language lang, Font titleFont, Order order, Font font, Document doc)
        {
            var addressTable = new PdfPTable(2) { RunDirection = GetDirection(lang) };
            addressTable.DefaultCell.Border = Rectangle.NO_BORDER;
            addressTable.WidthPercentage = 100f;
            addressTable.SetWidths(new[] { 50, 50 });

            //seller info
            PrintSellerInfo(billSettings, vendorId, lang, titleFont, order, font, addressTable);

            //shipping info
            PrintClientBillingInfo(lang, order, titleFont, font, addressTable);

            doc.Add(addressTable);
            doc.Add(new Paragraph(" "));
        }

        private void PrintClientBillingInfo(Language lang, Order order, Font titleFont, Font font, PdfPTable addressTable)
        {
            var billingAddress = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang)
            };
            billingAddress.DefaultCell.Border = Rectangle.NO_BORDER;

            //cell = new PdfPCell();
            //cell.Border = Rectangle.NO_BORDER;
            const string indent = "   ";

            billingAddress.AddCell(GetParagraph("Plugins.Misc.PdfBillingExtension.ClientInfo", lang, titleFont));

            if (_addressSettings.CompanyEnabled && !string.IsNullOrEmpty(order.BillingAddress.Company))
                billingAddress.AddCell(GetParagraph("PDFInvoice.Company", indent, lang, font, order.BillingAddress.Company));

            billingAddress.AddCell(GetParagraph("PDFInvoice.Name", indent, lang, font, order.BillingAddress.FirstName + " " + order.BillingAddress.LastName));
            if (_addressSettings.PhoneEnabled)
                billingAddress.AddCell(GetParagraph("PDFInvoice.Phone", indent, lang, font, order.BillingAddress.PhoneNumber));
            if (_addressSettings.FaxEnabled && !string.IsNullOrEmpty(order.BillingAddress.FaxNumber))
                billingAddress.AddCell(GetParagraph("PDFInvoice.Fax", indent, lang, font, order.BillingAddress.FaxNumber));
            if (_addressSettings.StreetAddressEnabled)
                billingAddress.AddCell(GetParagraph("PDFInvoice.Address", indent, lang, font, order.BillingAddress.Address1));
            if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(order.BillingAddress.Address2))
                billingAddress.AddCell(GetParagraph("PDFInvoice.Address2", indent, lang, font, order.BillingAddress.Address2));
            if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled ||
                _addressSettings.CountyEnabled || _addressSettings.ZipPostalCodeEnabled)
            {
                var addressLine = $"{indent}{order.BillingAddress.City}, " +
                    $"{(!string.IsNullOrEmpty(order.BillingAddress.County) ? $"{order.BillingAddress.County}, " : string.Empty)}" +
                    $"{(order.BillingAddress.StateProvince != null ? _localizationService.GetLocalized(order.BillingAddress.StateProvince, x => x.Name, lang.Id) : string.Empty)} " +
                    $"{order.BillingAddress.ZipPostalCode}";
                billingAddress.AddCell(new Paragraph(addressLine, font));
            }

            if (_addressSettings.CountryEnabled && order.BillingAddress.Country != null)
                billingAddress.AddCell(new Paragraph(indent + _localizationService.GetLocalized(order.BillingAddress.Country, x => x.Name, lang.Id),
                    font));

            //VAT number
            if (!string.IsNullOrEmpty(order.VatNumber))
                billingAddress.AddCell(GetParagraph("PDFInvoice.VATNumber", indent, lang, font, order.VatNumber));

            //custom attributes
            var customBillingAddressAttributes =
                _addressAttributeFormatter.FormatAttributes(order.BillingAddress.CustomAttributes);
            if (!string.IsNullOrEmpty(customBillingAddressAttributes))
            {
                //TODO: we should add padding to each line (in case if we have several custom address attributes)
                billingAddress.AddCell(
                    new Paragraph(indent + HtmlHelper.ConvertHtmlToPlainText(customBillingAddressAttributes, true, true), font));
            }                       

            addressTable.AddCell(billingAddress);
        }

        private void PrintSellerInfo(PdfExtendedSettings billSettings, int vendorId, Language lang, Font titleFont, Order order, Font font, PdfPTable addressTable)
        {
            const string indent = "   ";
            var sellerInfo = new PdfPTable(1) { RunDirection = GetDirection(lang) };
            sellerInfo.DefaultCell.Border = Rectangle.NO_BORDER;

            string formatText, text;

            // title
            sellerInfo.AddCell(GetParagraph("Plugins.Misc.PdfBillingExtension.SellerInfo", lang, titleFont));
            
            // company            
            sellerInfo.AddCell(GetTextParagraph(billSettings.BillingCompany, indent, font));
            
            // company info
            if (!string.IsNullOrWhiteSpace(billSettings.BillingCompanyInfo))
            {
                formatText = _localizationService.GetResource("Plugins.Misc.PdfBillingExtension.BillingCompanyInfo", lang.Id);
                text = $"{formatText}: {billSettings.BillingCompanyInfo}";
                sellerInfo.AddCell(GetTextParagraph(text, indent, font));
            }

            // company cde
            formatText = _localizationService.GetResource("Plugins.Misc.PdfBillingExtension.BillingCompanyCode", lang.Id);
            text = $"{formatText}: {billSettings.BillingCompanyCode}";
            sellerInfo.AddCell(GetTextParagraph(text, indent, font));

            // address 1 
            formatText = _localizationService.GetResource("Plugins.Misc.PdfBillingExtension.BillingCompanyAddress1", lang.Id);
            text = $"{formatText}: {billSettings.BillingCompanyAddress1}";
            sellerInfo.AddCell(GetTextParagraph(text, indent, font));

            // address 2
            if (!string.IsNullOrWhiteSpace(billSettings.BillingCompanyAddress2))
            {
                formatText = _localizationService.GetResource("Plugins.Misc.PdfBillingExtension.BillingCompanyAddress2", lang.Id);
                text = $"{formatText}: {billSettings.BillingCompanyAddress2}";
                sellerInfo.AddCell(GetTextParagraph(text, indent, font));
            }

            addressTable.AddCell(sellerInfo);
        }

        /// <summary>
        /// returns a paragraph with the received text. The caller is responsible for the text translation if necesary
        /// </summary>
        /// <param name="indent">Indent</param>
        /// <param name="font">Font</param>
        /// <returns>Paragraph</returns>
        private Paragraph GetTextParagraph(string text, string indent, Font font)
        {
            return new Paragraph(indent + text, font);
        }
    }
}
