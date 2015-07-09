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



public partial class Default_New : System.Web.UI.Page
{
    public string action1 = string.Empty;
    public string hash1 = string.Empty;

    protected void Page_Load(object sender, EventArgs e)
    {
        
        try
        {
            PayUMoneyHandler p = new PayUMoneyHandler("PO-00011", "100", "Mayank", "email@email.com", "34234234", "Apple");
            key.Value = p.MERCHANT_KEY;
            GoToPayUMoney(p);
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



    public void GoToPayUMoney(PayUMoneyHandler payU)
    {
        try
        {

            string[] hashVarsSeq;
            string hash_string = string.Empty;
            //"key|txnid|amount|productinfo|firstname|email|udf1|udf2|udf3|udf4|udf5|udf6|udf7|udf8|udf9|udf10"
            
            hashVarsSeq = ConfigurationManager.AppSettings["hashSequence"].Split('|'); // spliting hash sequence from config
            hash_string = "";
            hash_string = hash_string + ConfigurationManager.AppSettings["MERCHANT_KEY"] + "|";
            hash_string = hash_string + payU.ORDER_ID + "|";
            hash_string = hash_string + Convert.ToDecimal(payU.AMOUNT).ToString("g29") + "|";
            hash_string = hash_string + payU.FIRSTNAME+"|";
            hash_string = hash_string + payU.EMAIL+"|";
            hash_string = hash_string + payU.PHONE+"|";
            hash_string = hash_string + payU.lastName + "|";
            hash_string = hash_string + payU.u3 + "|";
            hash_string = hash_string + payU.u4 + "|";
            hash_string = hash_string + payU.u5 + "|";
            hash_string = hash_string + payU.u5 + "|";
            hash_string = hash_string + payU.u5 + "|";
            hash_string = hash_string + payU.u5 + "|";
            hash_string = hash_string + payU.u5 + "|";
            hash_string = hash_string + payU.u5;

            hash_string += ConfigurationManager.AppSettings["SALT"];// appending SALT

            hash1 = Generatehash512(hash_string).ToLower();         //generating hash
            action1 = ConfigurationManager.AppSettings["PAYU_BASE_URL"] + "/_payment";// setting URL

            if (!string.IsNullOrEmpty(hash1))
            {
                hash.Value = hash1;
                System.Collections.Hashtable data = new System.Collections.Hashtable(); // adding values in gash table for data post
                data.Add("hash", hash.Value);
                data.Add("txnid", payU.ORDER_ID);
                data.Add("key", payU.MERCHANT_KEY);
                string AmountForm = Convert.ToDecimal(payU.AMOUNT.Trim()).ToString("g29");// eliminating trailing zeros
                data.Add("amount", AmountForm);
                data.Add("firstname", payU.FIRSTNAME.Trim());
                data.Add("email", payU.EMAIL.Trim());
                data.Add("phone", payU.PHONE.Trim());
                data.Add("productinfo", payU.PRODUCT_INFO.Trim());
                data.Add("surl", payU.SUCCESS_URL.Trim());
                data.Add("furl", payU.FAILURE_URL.Trim());
                data.Add("lastname", payU.lastName.Trim());
                data.Add("curl", payU.cancleUrl.Trim());
                data.Add("address1", payU.address1.Trim());
                data.Add("address2", payU.address2.Trim());
                data.Add("city", payU.city.Trim());
                data.Add("state", payU.state.Trim());
                data.Add("country", payU.country.Trim());
                data.Add("zipcode", payU.zip.Trim());
                data.Add("udf1", payU.u1.Trim());
                data.Add("udf2", payU.u2.Trim());
                data.Add("udf3", payU.u3.Trim());
                data.Add("udf4", payU.u4.Trim());
                data.Add("udf5", payU.u5.Trim());
                data.Add("pg", payU.pg.Trim());
                data.Add("service_provider", payU.SERVICE_PROVIDER.Trim());


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

    private string PreparePOSTForm(string url, System.Collections.Hashtable data)      // post form
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
