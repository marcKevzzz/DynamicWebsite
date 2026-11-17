const shortDate = new Intl.DateTimeFormat('en-US', { month: 'short', day: 'numeric', timeZone: 'UTC' })
const fullDate = new Intl.DateTimeFormat('en-US', { month: 'short', day: 'numeric', year: 'numeric', timeZone: 'UTC' })
const monthDate = new Intl.DateTimeFormat('en-US', { month: 'long', year: 'numeric', timeZone: 'UTC' })
const shortMonth = new Intl.DateTimeFormat('en-US', { month: 'short', timeZone: 'UTC' })
const weekday = new Intl.DateTimeFormat('en-US', { weekday: 'short', timeZone: 'UTC' })
const number = new Intl.NumberFormat('en-US')
const signedNumber = new Intl.NumberFormat('en-US', { signDisplay: 'exceptZero' })

function utcDate(value) {
  return new Date(value + 'T00:00:00Z')
}

export function formatAdminDateRange(start, end) {
  return fullDate.formatRange(utcDate(start), utcDate(end))
}

export function attachAdminDatePicker(picker, onApply) {
  const trigger = picker.querySelector('.admin-date-toggle')
  const caption = picker.querySelector('[data-date-caption]')
  const form = picker.querySelector('form')
  const { start, end } = form.elements
  const error = form.querySelector('.admin-date-error')
  let committedStart = start.value
  let committedEnd = end.value

  function validateRange() {
    const invalid = Boolean(start.value && end.value && start.value > end.value)
    error.textContent = invalid ? 'Choose an end date on or after the start date.' : ''
    error.hidden = !invalid
    end.setAttribute('aria-invalid', String(invalid))
    return !invalid
  }

  form.addEventListener('input', validateRange)
  form.addEventListener('beforetoggle', event => {
    if (event.newState !== 'open') return
    start.value = committedStart
    end.value = committedEnd
    validateRange()
    const bounds = trigger.getBoundingClientRect()
    const width = Math.min(320, window.innerWidth - 28)
    form.style.setProperty('--date-left', Math.max(14, bounds.right - width) + 'px')
    form.style.setProperty('--date-top', Math.max(14, Math.min(bounds.bottom + 8, window.innerHeight - 250)) + 'px')
  })
  form.addEventListener('toggle', event => {
    if (event.newState === 'open') start.focus({ preventScroll: true })
  })
  form.addEventListener('submit', event => {
    event.preventDefault()
    if (!validateRange()) {
      end.focus()
      return
    }
    committedStart = start.value
    committedEnd = end.value
    onApply(committedStart, committedEnd)
    form.hidePopover()
    trigger.focus()
  })

  return text => { caption.textContent = text }
}

function createPoints(days, period) {
  const first = days[0]
  const last = days.at(-1)
  if (period === 'today') {
    const labels = ['12a', '3a', '6a', '9a', '12p', '3p', '6p', '9p', '12a']
    const slots = [3, 4, 5, 2, 6, 1, 7, 0]
    const points = labels.slice(0, -1).map((label, index) => ({
      label, detail: `${shortDate.format(utcDate(last.date))} · ${label}–${labels[index + 1]}`, count: 0,
    }))
    for (let index = 0; index < last.signups; index++) points[slots[index % slots.length]].count++
    return points
  }
  if (period === 'weekly' || days.length <= 7) {
    return days.map(day => ({
      label: days.length === 1 || period === 'weekly' ? weekday.format(utcDate(day.date)) : utcDate(day.date).getUTCDate(),
      detail: fullDate.format(utcDate(day.date)), count: day.signups,
    }))
  }
  if (first.date.slice(0, 7) === last.date.slice(0, 7) || days.length <= 42) {
    const points = []
    const size = Math.ceil(days.length / 7)
    for (let offset = 0; offset < days.length; offset += size) {
      const window = days.slice(offset, offset + size)
      points.push({
        label: utcDate(window[0].date).getUTCDate(),
        detail: formatAdminDateRange(window[0].date, window.at(-1).date),
        count: window.reduce((total, day) => total + day.signups, 0),
      })
    }
    return points
  }

  const months = new Map()
  for (const day of days) {
    const key = day.date.slice(0, 7)
    if (!months.has(key)) months.set(key, { start: day.date, end: day.date, count: 0 })
    const month = months.get(key)
    month.end = day.date
    month.count += day.signups
  }
  return [...months.values()].map(month => ({
    label: shortMonth.format(utcDate(month.start)),
    detail: period === 'monthly' ? monthDate.format(utcDate(month.start)) : formatAdminDateRange(month.start, month.end),
    count: month.count,
  }))
}

function updateMetric(metric, value, change, comparison, favorable, percent) {
  metric.querySelector('[data-metric-value]').textContent = number.format(value)
  metric.querySelector('[data-metric-change]').textContent = change === null ? '—' : signedNumber.format(change) + (percent ? '%' : '')
  metric.querySelector('[data-metric-comparison]').textContent = comparison
  metric.querySelector('[data-metric-trend]').dataset.positive = String(favorable)
  metric.querySelector('[data-metric-direction="up"]').hidden = !(change > 0)
  metric.querySelector('[data-metric-direction="down"]').hidden = !(change < 0)
}

export function attachAdminAnalytics() {
  const filters = document.querySelector('[data-analytics-filters]')
  const data = document.querySelector('[data-analytics-data]')
  if (!filters || !data) return

  const days = JSON.parse(data.textContent)
  const latest = days.at(-1)
  const dayIndex = new Map(days.map((day, index) => [day.date, index]))
  const metrics = [...document.querySelector('[data-analytics-metrics]').children]
  const signupLabel = metrics[3].querySelector('[data-metric-label]')
  const bars = document.querySelector('.admin-bars')
  const barTemplate = bars.firstElementChild
  const scale = [...document.querySelector('.admin-chart-scale').children]
  const chartRange = document.querySelector('[data-analytics-range]')
  const announcement = document.querySelector('[data-analytics-announcement]')
  const ring = document.querySelector('[data-analytics-ring]')
  const periodButtons = [...filters.querySelectorAll('[data-analytics-period]')]
  const segmented = filters.querySelector('[data-admin-segmented]')
  const datePicker = filters.querySelector('[data-admin-date-picker]')
  const dateToggle = datePicker?.querySelector('.admin-date-toggle')

  bars.addEventListener('click', event => {
    const point = event.target.closest('.admin-chart-point')
    if (!point) return
    const pinned = bars.querySelector('[aria-pressed="true"]')
    pinned?.setAttribute('aria-pressed', 'false')
    if (point !== pinned) point.setAttribute('aria-pressed', 'true')
  })
  bars.addEventListener('keydown', event => {
    if (event.key === 'Escape') bars.querySelector('[aria-pressed="true"]')?.setAttribute('aria-pressed', 'false')
  })

  function select(index) {
    periodButtons.forEach((button, position) => button.setAttribute('aria-pressed', String(position === index)))
    if (segmented) segmented.dataset.activeIndex = index
    dateToggle?.setAttribute('aria-pressed', String(index < 0))
  }

  function render(period, first, last, comparisonStart, comparisonEnd, comparisonLabel) {
    const window = days.slice(first, last + 1)
    const previousWindow = comparisonStart >= 0 ? days.slice(comparisonStart, comparisonEnd + 1) : []
    const end = window.at(-1)
    const previous = previousWindow.at(-1)
    const active = end.total - end.inactive
    const signupCount = window.reduce((total, day) => total + day.signups, 0)
    const previousSignups = previousWindow.reduce((total, day) => total + day.signups, 0)
    const signupChange = previousSignups ? Math.round((signupCount - previousSignups) * 100 / previousSignups) : null
    const comparisons = [
      previous ? end.total - previous.total : null,
      previous ? active - (previous.total - previous.inactive) : null,
      previous ? end.inactive - previous.inactive : null,
      signupChange,
    ]

    signupLabel.textContent = { today: 'Today’s sign-ups', weekly: 'Weekly sign-ups', monthly: 'Monthly sign-ups', range: 'Sign-ups' }[period]
    const values = [end.total, active, end.inactive, signupCount]
    metrics.forEach((metric, index) => {
      const change = comparisons[index]
      const favorable = index === 2 ? change < 0 : change > 0
      const comparison = index === 3 && previous && !previousSignups ? 'No earlier sign-ups' : previous ? comparisonLabel : 'No earlier data'
      updateMetric(metric, values[index], change, comparison, favorable, index === 3)
    })

    const plotDays = period === 'monthly' ? days : window
    const points = createPoints(plotDays, period)
    const step = Math.max(1, Math.ceil(Math.max(...points.map(point => point.count)) / 4))
    scale.forEach((label, index) => { label.textContent = step * (4 - index) })
    bars.style.setProperty('--plot-columns', points.length)
    bars.style.setProperty('--plot-maximum', step * 4)
    chartRange.textContent = formatAdminDateRange(plotDays[0].date, plotDays.at(-1).date)
    bars.replaceChildren(...points.map((point, index) => {
      const item = barTemplate.cloneNode(true)
      const count = number.format(point.count) + (point.count === 1 ? ' sign-up' : ' sign-ups')
      item.dataset.current = String(index === points.length - 1)
      const button = item.querySelector('button')
      button.setAttribute('aria-label', point.detail + ', ' + count)
      button.setAttribute('aria-pressed', 'false')
      item.style.setProperty('--bar-count', point.count)
      item.querySelector('[data-point-value]').textContent = number.format(point.count)
      item.querySelector('[data-point-label]').textContent = point.label
      item.querySelector('[data-point-date]').textContent = point.detail
      item.querySelector('[data-point-count]').textContent = count
      return item
    }))

    if (ring) {
      const share = end.total ? Math.floor(active * 100 / end.total) : 0
      ring.style.setProperty('--active-share', share + '%')
      ring.setAttribute('aria-label', share + ' percent active')
      ring.firstElementChild.textContent = share + '%'
      document.querySelector('[data-analytics-active]').textContent = number.format(active)
      document.querySelector('[data-analytics-inactive]').textContent = number.format(end.inactive)
    }
    announcement.textContent = 'Showing ' + formatAdminDateRange(window[0].date, end.date) + '. ' + signupCount + ' sign-ups.'
  }

  const today = days.length - 1
  const monthStart = dayIndex.get(latest.date.slice(0, 7) + '-01')
  const lastMonthStart = dayIndex.get(days[monthStart - 1].date.slice(0, 7) + '-01')
  periodButtons.forEach((button, index) => {
    button.addEventListener('click', () => {
      const period = button.dataset.analyticsPeriod
      select(index)
      if (period === 'today') render(period, today, today, today - 1, today - 1, 'vs. yesterday')
      if (period === 'weekly') render(period, today - 6, today, today - 13, today - 7, 'vs. last week')
      if (period === 'monthly') render(period, monthStart, today, lastMonthStart, monthStart - 1, 'vs. last month')
    })
    button.addEventListener('pointerenter', () => {
      if (segmented) segmented.dataset.hoverIndex = index
    })
  })
  segmented?.addEventListener('pointerleave', () => { delete segmented.dataset.hoverIndex })
  if (datePicker) {
    const setCaption = attachAdminDatePicker(datePicker, (start, end) => {
      const first = dayIndex.get(start)
      const last = dayIndex.get(end)
      select(-1)
      setCaption(formatAdminDateRange(start, end))
      render('range', first, last, first - (last - first + 1), first - 1, 'vs. prior period')
    })
    select(-1)
  }
}
