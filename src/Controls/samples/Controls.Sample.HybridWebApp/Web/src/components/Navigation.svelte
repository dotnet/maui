<script lang="ts">
  import { untrack } from 'svelte'
  import { navigationPages, availableFeatures } from '../catalog.ts'
  import type { Feature } from '../catalog.ts'
  import type { PlaygroundSession } from '../session.ts'

  let { session, compact, onselect = () => {} }: {
    session: PlaygroundSession; compact: () => boolean; onselect?: () => void
  } = $props()
  let search = $state('')
  let collapsed = $state<Record<string, boolean>>({})
  let navigation: HTMLElement
  const groups = ['Welcome', 'Essentials', 'App & windows', 'Bridge', 'Development']
  let matches = $derived(navigationPages.filter(feature =>
    `${feature.title} ${feature.category} ${feature.description}`.toLowerCase().includes(search.toLowerCase())))
  let selected = $derived($session.selected)
  $effect(() => {
    const group = selected.category
    untrack(() => { collapsed = { ...collapsed, [group]: false } })
  })

  function select(feature: Feature, button: HTMLButtonElement): void {
    session.select(feature)
    onselect()
    if (compact()) {
      navigation.ownerDocument.querySelector<HTMLElement>('#experiment')?.focus()
    } else {
      button.focus()
    }
  }
</script>

<aside class="catalog" aria-label="Feature catalog">
  <label for="search">Find a feature</label>
  <input id="search" type="search" placeholder="Search features…" autocomplete="off" bind:value={search} />
  <p id="catalog-count" class="secondary">
    {search ? `${matches.length} matching pages` : `${availableFeatures.length} experiments · 2 guides`}
  </p>
  <nav id="feature-list" aria-label="Experiments" bind:this={navigation}>
    {#each groups as group, index}
      {@const pages = matches.filter(page => page.category === group)}
      {@const expanded = !!search || !collapsed[group]}
      {#if pages.length}
        <section class="nav-group">
          <h3 class="group-heading">
            <button type="button" class="group-toggle" data-group={group} data-selected={$session.selected.category === group}
              aria-expanded={expanded} aria-controls={`nav-group-${index}`}
              onclick={() => { collapsed = { ...collapsed, [group]: !collapsed[group] } }}>
              <svg class="group-chevron" viewBox="0 0 16 16" fill="none" aria-hidden="true" focusable="false">
                <path d="m6 4 4 4-4 4" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round" />
              </svg>
              <span>{group}</span>
            </button>
          </h3>
          <div id={`nav-group-${index}`} hidden={!expanded}>
            {#each pages as feature (feature.id)}
              <button
                type="button"
                data-feature={feature.id}
                aria-current={$session.selected.id === feature.id ? 'page' : undefined}
                onclick={event => select(feature, event.currentTarget)}
              >
                <span>{feature.title}</span>
                <small>{feature.description.split(/\.\s/)[0]}</small>
              </button>
            {/each}
          </div>
        </section>
      {/if}
    {/each}
    {#if !matches.length}
      <p>No matching features. Try a broader search or clear the filters.</p>
    {/if}
  </nav>
</aside>
