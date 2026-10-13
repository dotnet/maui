<script lang="ts">
  import { untrack } from 'svelte'
  import { navigationPages, availableFeatures } from '../catalog.ts'
  import type { Feature } from '../catalog.ts'
  import type { PlaygroundSession } from '../session.ts'

  let { session, compact }: { session: PlaygroundSession; compact: () => boolean } = $props()
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
          <button type="button" class="group-toggle" data-group={group} data-selected={$session.selected.category === group}
            aria-expanded={expanded} aria-controls={`nav-group-${index}`}
            onclick={() => { collapsed = { ...collapsed, [group]: !collapsed[group] } }}>
            <span>{group}</span><span class="group-state">{expanded ? 'Collapse' : 'Expand'}</span>
          </button>
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
