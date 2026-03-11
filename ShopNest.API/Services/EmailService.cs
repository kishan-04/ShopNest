using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using ShopNest.API.DTOs;

namespace ShopNest.API.Services;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendOrderConfirmationAsync(string toEmail, string firstName, OrderDto order)
    {
        var emailSettings = _config.GetSection("EmailSettings");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            emailSettings["SenderName"],
            emailSettings["SenderEmail"]));
        message.To.Add(new MailboxAddress(firstName, toEmail));
        message.Subject = $"✅ Order #{order.Id} Confirmed — ShopNest";

        // Build HTML email body
        message.Body = new TextPart("html")
        {
            Text = BuildEmailBody(firstName, order)
        };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(
            emailSettings["Host"],
            int.Parse(emailSettings["Port"]!),
            SecureSocketOptions.StartTls);

        await smtp.AuthenticateAsync(
            emailSettings["SenderEmail"],
            emailSettings["Password"]);

        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);
    }

    public async Task SendContactEmailAsync(string fromEmail, string fromName, ContactDto dto)
    {
        var emailSettings = _config.GetSection("EmailSettings");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, emailSettings["SenderEmail"]));
        message.To.Add(new MailboxAddress("Admin", emailSettings["SenderEmail"]));
        message.Subject = $"📩 Contact Us: {dto.Subject}";

        // Set ReplyTo so admin can reply directly to customer
        message.ReplyTo.Add(new MailboxAddress(fromName, fromEmail));

        message.Body = new TextPart("html")
        {
            Text = $@"
        <!DOCTYPE html>
        <html>
        <body style='font-family: Arial, sans-serif; background: #f4f4f4; padding: 20px;'>
            <div style='max-width: 600px; margin: 0 auto; background: white;
                        border-radius: 10px; overflow: hidden;
                        box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>

                <!-- Header -->
                <div style='background: #2c3e50; color: white;
                            padding: 30px; text-align: center;'>
                    <h1 style='margin: 0; font-size: 32px; letter-spacing: 2px;'>
                        Shop<span style='color: #3498db;'>Nest</span>
                    </h1>
                    <p style='margin: 10px 0 0; color: #bdc3c7;'>Customer Message</p>
                </div>

                <!-- Body -->
                <div style='padding: 30px;'>
                    <h2 style='color: #2c3e50;'>New Message from Customer</h2>

                    <!-- Customer Info -->
                    <div style='background: #f8f9fa; padding: 15px;
                                border-radius: 8px; margin: 20px 0;'>
                        <p style='margin: 0;'>
                            <strong>👤 Name:</strong> {fromName}
                        </p>
                        <p style='margin: 5px 0 0;'>
                            <strong>📧 Email:</strong> {fromEmail}
                        </p>
                        <p style='margin: 5px 0 0;'>
                            <strong>📌 Subject:</strong> {dto.Subject}
                        </p>
                    </div>

                    <!-- Message -->
                    <h3 style='color: #2c3e50;'>💬 Message:</h3>
                    <div style='background: #f8f9fa; padding: 15px;
                                border-radius: 8px; color: #555;
                                line-height: 1.6;'>
                        {dto.Message}
                    </div>

                    <div style='margin-top: 20px; padding: 15px;
                                background: #eaf4fb; border-radius: 8px;'>
                        <p style='margin: 0; color: #2980b9; font-size: 13px;'>
                            💡 You can reply directly to this email
                            to respond to the customer.
                        </p>
                    </div>
                </div>

                <!-- Footer -->
                <div style='background: #f8f9fa; padding: 20px;
                            text-align: center; color: #888;'>
                    <p style='margin: 0; font-size: 12px;'>
                        ShopNest Admin Panel — Customer Support
                    </p>
                </div>

            </div>
        </body>
        </html>"
        };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(
            emailSettings["Host"],
            int.Parse(emailSettings["Port"]!),
            SecureSocketOptions.StartTls);

        await smtp.AuthenticateAsync(
            emailSettings["SenderEmail"],
            emailSettings["Password"]);

        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);
    }

    private string BuildEmailBody(string firstName, OrderDto order)
    {
        // Build items rows
        var itemsHtml = string.Empty;
        foreach (var item in order.Items)
        {
            itemsHtml += $@"
                <tr>
                    <td style='padding: 8px; border-bottom: 1px solid #eee;'>
                        {item.ProductName}
                        <small style='color: #888;'>({item.CategoryName})</small>
                    </td>
                    <td style='padding: 8px; border-bottom: 1px solid #eee; text-align: center;'>
                        x{item.Quantity}
                    </td>
                    <td style='padding: 8px; border-bottom: 1px solid #eee; text-align: right;'>
                        ${item.Subtotal}
                    </td>
                </tr>";
        }

        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='utf-8'>
        </head>
        <body style='font-family: Arial, sans-serif; background: #f4f4f4; padding: 20px;'>

            <div style='max-width: 600px; margin: 0 auto; background: white;
                        border-radius: 10px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>

                <!-- Header -->
                <div style='background: #2c3e50; color: white; padding: 30px; text-align: center;'>
                    <h1 style='margin: 0;'> Shop<span style='color: #3498db;'>Nest</span></h1>
                    <p style='margin: 10px 0 0;'>Order Confirmation</p>
                </div>

                <!-- Body -->
                <div style='padding: 30px;'>
                    <h2 style='color: #2c3e50;'>Hi {firstName}! 👋</h2>
                    <p style='color: #555;'>
                        Thank you for your order! We have received it and
                        it is being processed.
                    </p>

                    <!-- Order Info -->
                    <div style='background: #f8f9fa; padding: 15px;
                                border-radius: 8px; margin: 20px 0;'>
                        <p style='margin: 0;'>
                            <strong>Order #:</strong> {order.Id}
                        </p>
                        <p style='margin: 5px 0 0;'>
                            <strong>Date:</strong>
                            {order.CreatedAt:dd MMM yyyy}
                        </p>
                        <p style='margin: 5px 0 0;'>
                            <strong>Status:</strong>
                            <span style='color: #e67e22;'>{order.Status}</span>
                        </p>
                    </div>

                    <!-- Items Table -->
                    <h3 style='color: #2c3e50;'>📦 Order Items</h3>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <thead>
                            <tr style='background: #f8f9fa;'>
                                <th style='padding: 10px; text-align: left;'>Product</th>
                                <th style='padding: 10px; text-align: center;'>Qty</th>
                                <th style='padding: 10px; text-align: right;'>Price</th>
                            </tr>
                        </thead>
                        <tbody>
                            {itemsHtml}
                        </tbody>
                        <tfoot>
                            <tr>
                                <td colspan='2'
                                    style='padding: 10px; text-align: right;
                                           font-weight: bold;'>
                                    Total:
                                </td>
                                <td style='padding: 10px; text-align: right;
                                           font-weight: bold; color: #2980b9;
                                           font-size: 18px;'>
                                    ${order.TotalAmount}
                                </td>
                            </tr>
                        </tfoot>
                    </table>

                    <!-- Shipping Address -->
                    <div style='margin-top: 20px; padding: 15px;
                                background: #f8f9fa; border-radius: 8px;'>
                        <p style='margin: 0;'>
                            <strong>📍 Shipping Address:</strong><br>
                            {order.ShippingAddress}
                        </p>
                    </div>

                </div>

                <!-- Footer -->
                <div style='background: #f8f9fa; padding: 20px;
                            text-align: center; color: #888;'>
                    <p style='margin: 0;'>
                        Thank you for shopping with ShopNest! 🛍️
                    </p>
                    <p style='margin: 5px 0 0; font-size: 12px;'>
                        If you have any questions contact us at shopnestpvtltd@gmail.com
                    </p>
                </div>

            </div>

        </body>
        </html>";
    }
}