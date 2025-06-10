import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'sum'
})
export class SumPipe implements PipeTransform {

  transform(items: any[], attr: string): number {
    if (!items || !items.length) {
      return 0;
    }

    return items.reduce((sum, item) => sum + (parseFloat(item[attr]) || 0), 0);
  }
}
