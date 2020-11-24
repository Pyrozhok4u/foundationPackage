var BrowserVersionPlugin = 
 {
   GetBrowserVersion: function()
   {
 
     var ua = window.navigator.userAgent;
     var tem;
       var M = ua.match(/(opera|chrome|safari|firefox|msie|trident(?=\/))\/?\s*(\d+)/i) || [];
 
       var returnString = "test";
    
       if(/trident/i.test(M[1]))
       {
           tem=  /\brv[ :]+(\d+)/g.exec(ua) || [];
           returnString = 'IE '+(tem[1] || '');
       }
 
       if(M[1]=== 'Chrome')
       {
           tem= ua.match(/\b(OPR|Edge)\/(\d+)/);
           if(tem!= null) 
             returnString = tem.slice(1).join(' ').replace('OPR', 'Opera');
       }
 
       M= M[2]? [M[1], M[2]]: [window.navigator.appName, window.navigator.appVersion, '-?'];
 
       if((tem= ua.match(/version\/(\d+)/i))!= null) M.splice(1, 1, tem[1]);
         returnString = M.join(' ');
     
       var buffer = _malloc(lengthBytesUTF8(returnString) + 1);
       stringToUTF8(returnString, buffer, returnString.length + 1);
         return buffer;
     }
 };
 
 mergeInto(LibraryManager.library, BrowserVersionPlugin);