using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.IO;



public partial class _Default : System.Web.UI.Page
{
    public string action1 = string.Empty;
    public string hash1 = string.Empty;
    public string txnid1 = string.Empty;
    PayUMoneyHandler payU =  null;
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {

            payU = new PayUMoneyHandler("ORDERID", "200", "FNAME", "EMAIL", "345345", "INFO");

            //key.Value = payU.MERCHANT_KEY;

            Button1_Click(sender, e);

        }
        catch (Exception ex)
        {
            Response.Write("<span style='color:red'>" + ex.Message + "</span>");

        }
       
    }



    public string Generatehash512(string text)
    {

        byte[] message = Encoding.UTF8.GetBytes(text);

            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            SHA512Managed hashString = new SHA512Managed();
            string hex = "";
            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;

    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        try
        { 

        string[] hashVarsSeq;
        string hash_string = string.Empty;


        if (string.IsNullOrEmpty(Request.Form["txnid"])) // generating txnid
        {
            Random rnd = new Random();
            string strHash = Generatehash512(rnd.ToString() + DateTime.Now);
            txnid1 = strHash.ToString().Substring(0, 20);
            
        }
        else
        {
            txnid1 = Request.Form["txnid"];
        }
        if (string.IsNullOrEmpty(Request.Form["hash"])) // generating hash value
        {
            if (
                string.IsNullOrEmpty(payU.MERCHANT_KEY) ||
                string.IsNullOrEmpty(payU.ORDER_ID) ||
                string.IsNullOrEmpty(payU.AMOUNT) ||
                string.IsNullOrEmpty(payU.FIRSTNAME) ||
                string.IsNullOrEmpty(payU.EMAIL) ||
                string.IsNullOrEmpty(payU.PHONE) ||
                string.IsNullOrEmpty(payU.PRODUCT_INFO) ||
                string.IsNullOrEmpty(payU.SUCCESS_URL) ||
                string.IsNullOrEmpty(payU.FAILURE_URL) ||
				string.IsNullOrEmpty(payU.SERVICE_PROVIDER) 
                )
            {
                //error

                //frmError.Visible = true;
                return;
            }

            else
            {
               // frmError.Visible = false;
                hashVarsSeq = ConfigurationManager.AppSettings["hashSequence"].Split('|'); // spliting hash sequence from config
                hash_string = "";
                //Sc20Zv|e06a31fd11aee53fe4cb|200|Apple Iphone 6|Mayank|mayankjhawar18@gmail.com|||||||||||

                hash_string = hash_string + payU.MERCHANT_KEY + "|" + payU.ORDER_ID + "|" + payU.AMOUNT + "|" + payU.PRODUCT_INFO + "|" + payU.FIRSTNAME + "|" + payU.EMAIL + "|||||||||||";
                hash_string += ConfigurationManager.AppSettings["SALT"];// appending SALT

                hash1 = Generatehash512(hash_string).ToLower();         //generating hash
                action1 = ConfigurationManager.AppSettings["PAYU_BASE_URL"] + "/_payment";// setting URL

            }


        }

        else if (!string.IsNullOrEmpty(Request.Form["hash"]))
        {
            hash1 = Request.Form["hash"];
            action1 = ConfigurationManager.AppSettings["PAYU_BASE_URL"] + "/_payment";

        }

      


        if (!string.IsNullOrEmpty(hash1))
        {
            //hash.Value = hash1; 
            //txnid.Value = txnid1;

            System.Collections.Hashtable data = new System.Collections.Hashtable(); // adding values in gash table for data post
            data.Add("hash", hash1);
            data.Add("txnid", payU.ORDER_ID);
            data.Add("key", payU.MERCHANT_KEY);
            string AmountForm = Convert.ToDecimal(payU.AMOUNT).ToString("g29");// eliminating trailing zeros
            data.Add("amount", AmountForm);
            data.Add("firstname", payU.FIRSTNAME);
            data.Add("email", payU.EMAIL);
            data.Add("phone", payU.PHONE);
            data.Add("productinfo", payU.PRODUCT_INFO.Trim());
            data.Add("surl", payU.SUCCESS_URL.Trim());
            data.Add("furl", payU.FAILURE_URL.Trim());
            data.Add("lastname", payU.lastName.Trim());
            data.Add("curl", "");
            data.Add("address1", "");
            data.Add("address2", "");
            data.Add("city", "");
            data.Add("state", "");
            data.Add("country","");
            data.Add("zipcode", "");
            data.Add("udf1", "");
            data.Add("udf2", "");
            data.Add("udf3","");
            data.Add("udf4", "");
            data.Add("udf5", "");
            data.Add("pg", "");
			data.Add("service_provider", payU.SERVICE_PROVIDER);


            string strForm = PreparePOSTForm(action1, data);
            Page.Controls.Add(new LiteralControl(strForm));

        }

        else
        {
            //no hash
        
        }

    }

    catch (Exception ex)

    {
        Response.Write("<span style='color:red'>" + ex.Message + "</span>");
    
    }



    }
    private string PreparePOSTForm(string url,  System.Collections.Hashtable  data)      // post form
{
    //Set a name for the form
    string formID = "PostForm";
    //Build the form using the specified data to be posted.
    StringBuilder strForm = new StringBuilder();
    strForm.Append("<form id=\"" + formID + "\" name=\"" + 
                   formID + "\" action=\"" + url + 
                   "\" method=\"POST\">");

    foreach (System.Collections.DictionaryEntry key in data)
    {

        strForm.Append("<input type=\"hidden\" name=\"" + key.Key + 
                       "\" value=\"" + key.Value + "\">");
    }
        

    strForm.Append("</form>");
    //Build the JavaScript which will do the Posting operation.
    StringBuilder strScript = new StringBuilder();
    strScript.Append("<script language='javascript'>");
    strScript.Append("var v" + formID + " = document." + 
                     formID + ";");
    strScript.Append("v" + formID + ".submit();");
    strScript.Append("</script>");
    //Return the form and the script concatenated.
    //(The order is important, Form then JavaScript)
    return strForm.ToString() + strScript.ToString();
}

    

   
    
}
