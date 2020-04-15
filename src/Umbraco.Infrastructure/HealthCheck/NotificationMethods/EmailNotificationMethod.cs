﻿using System;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.NotificationMethods
{
    [HealthCheckNotificationMethod("email")]
    public class EmailNotificationMethod : NotificationMethodBase
    {
        private readonly ILocalizedTextService _textService;
        private readonly IRuntimeState _runtimeState;
        private readonly ILogger _logger;
        private readonly IGlobalSettings _globalSettings;
        private readonly IContentSettings _contentSettings;

        public EmailNotificationMethod(ILocalizedTextService textService, IRuntimeState runtimeState, ILogger logger, IGlobalSettings globalSettings, IHealthChecksSettings healthChecksSettings, IContentSettings contentSettings) : base(healthChecksSettings)
        {
            var recipientEmail = Settings?["recipientEmail"]?.Value;
            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                Enabled = false;
                return;
            }

            RecipientEmail = recipientEmail;

            _textService = textService ?? throw new ArgumentNullException(nameof(textService));
            _runtimeState = runtimeState;
            _logger = logger;
            _globalSettings = globalSettings;
            _contentSettings = contentSettings ?? throw new ArgumentNullException(nameof(contentSettings));
        }

        public string RecipientEmail { get; }

        public override async Task SendAsync(HealthCheckResults results, CancellationToken token)
        {
            if (ShouldSend(results) == false)
            {
                return;
            }

            if (string.IsNullOrEmpty(RecipientEmail))
            {
                return;
            }

            var message = _textService.Localize("healthcheck/scheduledHealthCheckEmailBody", new[]
            {
                DateTime.Now.ToShortDateString(),
                DateTime.Now.ToShortTimeString(),
                results.ResultsAsHtml(Verbosity)
            });

            // Include the umbraco Application URL host in the message subject so that
            // you can identify the site that these results are for.
            var host = _runtimeState.ApplicationUrl;

            var subject = _textService.Localize("healthcheck/scheduledHealthCheckEmailSubject", new[] { host.ToString() });

            var mailSender = new EmailSender(_globalSettings);
            using (var mailMessage = CreateMailMessage(subject, message))
            {
                await mailSender.SendAsync(mailMessage);
            }
        }

        private MailMessage CreateMailMessage(string subject, string message)
        {
            var to = _contentSettings.NotificationEmailAddress;

            if (string.IsNullOrWhiteSpace(subject))
                subject = "Umbraco Health Check Status";

            return new MailMessage(to, RecipientEmail, subject, message)
            {
                IsBodyHtml = message.IsNullOrWhiteSpace() == false && message.Contains("<") && message.Contains("</")
            };
        }
    }
}