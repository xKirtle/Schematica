using System.Collections.Generic;
using Schematica.Common.UI.UIElements;

namespace Schematica.Common;

public class NaturalComparer : IComparer<UIAccordionItem>
{
    public int Compare(UIAccordionItem item1, UIAccordionItem item2) {
        string x = item1.Title;
        string y = item2.Title;
        
        if (x == null && y == null)
            return 0;
        if (x == null)
            return -1;
        if (y == null)
            return 1;

        int lx = x.Length, ly = y.Length;

        int mx = 0, my = 0;
        for (; mx < lx && my < ly; mx++, my++) {
            if (char.IsDigit(x[mx]) && char.IsDigit(y[my])) {
                long vx = 0, vy = 0;

                for (; mx < lx && char.IsDigit(x[mx]); mx++)
                    vx = vx * 10 + x[mx] - '0';

                for (; my < ly && char.IsDigit(y[my]); my++)
                    vy = vy * 10 + y[my] - '0';

                if (vx != vy)
                    return vx > vy ? 1 : -1;
            }

            if (mx < lx && my < ly && x[mx] != y[my])
                return x[mx] > y[my] ? 1 : -1;
        }

        return lx - mx - (ly - my);
    }
}