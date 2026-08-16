import { Component, Input, OnChanges, OnDestroy, ViewChild, ElementRef, SimpleChanges, inject, effect, afterNextRender } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import * as am5 from '@amcharts/amcharts5';
import * as am5xy from '@amcharts/amcharts5/xy';
import am5themes_Animated from '@amcharts/amcharts5/themes/Animated';
import { LanguageService } from '../../services/language';

export interface BarChartDatum
{
    name: string;
    value: number;
}

@Component({
  selector: 'app-employee-bar-chart',
  imports: [MatIconModule],
  templateUrl: './employee-bar-chart.html',
  styleUrl: './employee-bar-chart.css',
})
export class EmployeeBarChartComponent implements OnChanges, OnDestroy
{
  @Input() data: BarChartDatum[] = [];
  @Input() categoryLabel = 'Category';
  @Input() valueLabel = 'Employees';
  @Input() noDataLabel = 'No data to display.';

  @ViewChild('chartDiv', { static: true }) chartDivRef!: ElementRef<HTMLDivElement>;

  private languageService = inject(LanguageService);

  private root: am5.Root | null = null;
  private series: am5xy.ColumnSeries | null = null;
  private xAxis: am5xy.CategoryAxis<am5xy.AxisRenderer> | null = null;
  private viewInitialized = false;

  get isEmpty(): boolean
  {
    return !this.data || this.data.length === 0;
  }

  constructor()
  {

    afterNextRender(() => {
      this.viewInitialized = true;
      this.buildChart();
    });

    effect(() => {
      this.languageService.dir();
      if (this.viewInitialized) {
        this.buildChart();
      }
    });
  }

  ngOnChanges(changes: SimpleChanges): void
  {
    if (!this.viewInitialized) {
      return;
    }

    if (changes['data']) {
      this.updateData();
    }

    if (changes['categoryLabel'] || changes['valueLabel']) {
      this.updateTooltipText();
    }
  }

  ngOnDestroy(): void
  {
    this.root?.dispose();
    this.root = null;
  }

  private buildChart(): void
  {
    this.root?.dispose();
    this.root = null;
    this.series = null;
    this.xAxis = null;

    if (this.isEmpty) {
      return;
    }

    const root = am5.Root.new(this.chartDivRef.nativeElement);
    root.setThemes([am5themes_Animated.new(root)]);

    const isRtl = this.languageService.dir() === 'rtl';
    (root as unknown as { rtl: boolean }).rtl = isRtl;

    const chart = root.container.children.push(
      am5xy.XYChart.new(root, {
        panX: false,
        panY: false,
        wheelX: 'none',
        wheelY: 'none'
      })
    );

    chart.set('cursor', am5xy.XYCursor.new(root, { behavior: 'none' }));

    const xAxis = chart.xAxes.push(
      am5xy.CategoryAxis.new(root, {
        categoryField: 'name',
        renderer: am5xy.AxisRendererX.new(root, { minGridDistance: 30 }),
        tooltip: am5.Tooltip.new(root, {})
      })
    );

    const yAxis = chart.yAxes.push(
      am5xy.ValueAxis.new(root, {
        min: 0,
        extraMax: 0.1,
        renderer: am5xy.AxisRendererY.new(root, {})
      })
    );

    const series = chart.series.push(
      am5xy.ColumnSeries.new(root, {
        name: 'Employees',
        xAxis,
        yAxis,
        valueYField: 'value',
        categoryXField: 'name',
        tooltip: am5.Tooltip.new(root, {})
      })
    );

    series.get('tooltip')!.label.setAll({
      html: this.buildTooltipHtml(isRtl)
    });

    series.columns.template.setAll({
      cornerRadiusTL: 6,
      cornerRadiusTR: 6,
      strokeOpacity: 0,
      fillOpacity: 0.85,
      fill: am5.color(0x3F51B5)
    });

    series.columns.template.states.create('hover', {
      fillOpacity: 1
    });

    this.root = root;
    this.xAxis = xAxis;
    this.series = series;

    this.updateData();
    this.updateTooltipText();

    series.appear(800, 100);
    chart.appear(800, 100);
  }

  private updateData(): void
  {

    if (!this.root || this.isEmpty) {
      this.buildChart();
      return;
    }

    this.xAxis!.data.setAll(this.data);
    this.series!.data.setAll(this.data);
  }

  private updateTooltipText(): void
  {
    const tooltip = this.series?.get('tooltip');
    if (!tooltip) {
      return;
    }

    const isRtl = this.languageService.dir() === 'rtl';
    tooltip.label.set('html', this.buildTooltipHtml(isRtl));
  }

  private buildTooltipHtml(isRtl: boolean): string
  {
    const direction = isRtl ? 'rtl' : 'ltr';
    const align = isRtl ? 'right' : 'left';
    return `<div style="direction:${direction}; text-align:${align}; white-space:nowrap;">${this.categoryLabel}: {categoryX}<br>${this.valueLabel}: {valueY}</div>`;
  }
}